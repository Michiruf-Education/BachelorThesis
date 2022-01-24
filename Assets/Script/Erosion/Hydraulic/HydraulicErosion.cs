using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class HydraulicErosion : IErosion
{
    private readonly HydraulicErosionSettings settings;
    private readonly Random random;

    public HydraulicErosion(HydraulicErosionSettings settings, int seed)
    {
        this.settings = settings;
        random = new Random(seed);
    }

    // Indices and weights of erosion brush precomputed for every node
    int[][] erosionBrushIndices;
    float[][] erosionBrushWeights;


    public void Erode(FloatField heightMap, FloatField hardnessMap)
    {
        if (erosionBrushIndices == null)
            InitializeBrushIndices(heightMap.width, settings.erosionRadius);


        // Create water droplet at random point on map
        var droplet = new Droplet(
            random.Next(0, heightMap.width - 1),
            random.Next(0, heightMap.height - 1),
            settings.initialSpeed,
            settings.initialWaterVolume);

        for (var lifetime = 0; lifetime < settings.maxDropletLifetime; lifetime++)
        {
            var originalDroplet = new Droplet(droplet);

            // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
            var heightAndGradient = HeightAndGradient.Calculate(heightMap, droplet);

            // Update the droplet's direction and position (move position 1 unit regardless of speed)
            droplet.direction += new Vector2(
                droplet.direction.x * settings.inertia - heightAndGradient.gradientX * (1 - settings.inertia),
                droplet.direction.y * settings.inertia - heightAndGradient.gradientY * (1 - settings.inertia)
            );
            droplet.direction.Normalize();
            droplet.position += droplet.direction;

            // Stop simulating droplet if it's not moving or has flowed over edge of map
            if (droplet.direction.x == 0 && droplet.direction.y == 0 ||
                droplet.position.x < 0 || droplet.position.x >= heightMap.width - 1 ||
                droplet.position.y < 0 || droplet.position.y >= heightMap.height - 1)
                break;

            // Find the droplet's new height and calculate the deltaHeight
            var newHeight = HeightAndGradient.Calculate(heightMap, droplet).height;
            var deltaHeight = newHeight - heightAndGradient.height;

            // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
            var sedimentCapacity = Mathf.Max(
                -deltaHeight * droplet.speed * droplet.water * settings.sedimentCapacityFactor,
                settings.minSedimentCapacity);

            // If carrying more sediment than capacity, or if flowing uphill:
            if (droplet.sediment > sedimentCapacity || deltaHeight > 0)
            {
                var cellOffset = originalDroplet.cellOffset;

                // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                var amountToDeposit = deltaHeight > 0
                    ? Mathf.Min(deltaHeight, droplet.sediment)
                    : (droplet.sediment - sedimentCapacity) * settings.depositSpeed;
                droplet.sediment -= amountToDeposit;

                // Add the sediment to the four nodes of the current cell using bilinear interpolation
                // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                var dropletIndex = originalDroplet.CalculateIndex(heightMap);
                var dropletCell = originalDroplet.cellPositionInt;
                heightMap.values[dropletIndex] += amountToDeposit * (1 - cellOffset.x) * (1 - cellOffset.y);
                heightMap.values[dropletIndex + 1] += amountToDeposit * cellOffset.x * (1 - cellOffset.y);
                heightMap.values[dropletIndex + heightMap.width] += amountToDeposit * (1 - cellOffset.x) * cellOffset.y;
                heightMap.values[dropletIndex + heightMap.width + 1] += amountToDeposit * cellOffset.x * cellOffset.y;
                // heightMap.BlendValue(dropletCell.x, dropletCell.y, BlendMode.Add, amountToDeposit * (1 - cellOffset.x) * (1 - cellOffset.y));
                // heightMap.BlendValue(dropletCell.x + 1, dropletCell.y, BlendMode.Add, amountToDeposit * cellOffset.x * (1 - cellOffset.y));
                // heightMap.BlendValue(dropletCell.x, dropletCell.y + 1, BlendMode.Add, amountToDeposit * (1 - cellOffset.x) * cellOffset.y);
                // heightMap.BlendValue(dropletCell.x + 1, dropletCell.y + 1, BlendMode.Add, amountToDeposit * cellOffset.x * cellOffset.y);
            }
            else
            {
                // Erode a fraction of the droplet's current carry capacity.
                // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                var amountToErode = Mathf.Min(
                    (sedimentCapacity - droplet.sediment) * settings.erodeSpeed,
                    -deltaHeight);

                // Use erosion brush to erode from all nodes inside the droplet's erosion radius
                var dropletIndex = originalDroplet.CalculateIndex(heightMap);
                for (var brushPointIndex = 0; brushPointIndex < erosionBrushIndices[dropletIndex].Length; brushPointIndex++)
                {
                    var nodeIndex = erosionBrushIndices[dropletIndex][brushPointIndex];
                    var weighedErodeAmount = amountToErode * erosionBrushWeights[dropletIndex][brushPointIndex];
                    var deltaSediment = heightMap.values[nodeIndex] < weighedErodeAmount ? heightMap.values[nodeIndex] : weighedErodeAmount;
                    heightMap.values[nodeIndex] -= deltaSediment;
                    droplet.sediment += deltaSediment;
                }
            }

            // Update droplet's speed and water content
            droplet.speed = Mathf.Sqrt(droplet.speed * droplet.speed + deltaHeight * settings.gravity);
            droplet.water *= 1 - settings.evaporateSpeed;
        }
    }

    void InitializeBrushIndices(int mapSize, int radius)
    {
        erosionBrushIndices = new int[mapSize * mapSize][];
        erosionBrushWeights = new float[mapSize * mapSize][];

        int[] xOffsets = new int[radius * radius * 4];
        int[] yOffsets = new int[radius * radius * 4];
        float[] weights = new float[radius * radius * 4];
        float weightSum = 0;
        int addIndex = 0;

        for (int i = 0; i < erosionBrushIndices.GetLength(0); i++)
        {
            int centreX = i % mapSize;
            int centreY = i / mapSize;

            if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 || centreX >= mapSize - radius)
            {
                weightSum = 0;
                addIndex = 0;
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        float sqrDst = x * x + y * y;
                        if (sqrDst < radius * radius)
                        {
                            int coordX = centreX + x;
                            int coordY = centreY + y;

                            if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                            {
                                float weight = 1 - Mathf.Sqrt(sqrDst) / radius;
                                weightSum += weight;
                                weights[addIndex] = weight;
                                xOffsets[addIndex] = x;
                                yOffsets[addIndex] = y;
                                addIndex++;
                            }
                        }
                    }
                }
            }

            int numEntries = addIndex;
            erosionBrushIndices[i] = new int[numEntries];
            erosionBrushWeights[i] = new float[numEntries];

            for (int j = 0; j < numEntries; j++)
            {
                erosionBrushIndices[i][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                erosionBrushWeights[i][j] = weights[j] / weightSum;
            }
        }
    }
}
