using System;
using UnityEngine;
using Random = System.Random;

public class HydraulicErosion : IErosion
{
    private readonly HydraulicErosionSettings s;
    private readonly Random random;

    private FloatField heightMap;
    private FloatField hardnessMap;

    // Indices and weights of erosion brush precomputed for every node
    private int[][] erosionBrushIndices;
    private float[][] erosionBrushWeights;

    public HydraulicErosion(HydraulicErosionSettings settings, int seed)
    {
        s = settings;
        random = new Random(seed);
    }

    public void Init(FloatField heightMap, FloatField hardnessMap)
    {
        this.heightMap = heightMap;
        this.hardnessMap = hardnessMap;
        // if (heightMap.width != hardnessMap.width || heightMap.height != hardnessMap.height)
        //     throw new Exception("HeightMap and HardnessMap not of same bounds");
        InitializeBrushIndices(heightMap.width, s.erosionRadius);
    }

    public void ErodeStep()
    {
        // Create water droplet at random point on map
        var droplet = new Droplet(
            random.Next(0, heightMap.width - 1),
            random.Next(0, heightMap.height - 1),
            s.initialSpeed,
            s.initialWaterVolume);

        for (var lifetime = 0; lifetime < s.maxDropletLifetime; lifetime++)
        {
            var originalDroplet = new Droplet(droplet);

            // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
            var heightAndGradient = HeightAndGradient.Calculate(heightMap, droplet);

            // Update the droplet's direction and position (move position 1 unit regardless of speed)
            droplet.direction += new Vector2(
                droplet.direction.x * s.inertia - heightAndGradient.gradientX * (1 - s.inertia),
                droplet.direction.y * s.inertia - heightAndGradient.gradientY * (1 - s.inertia)
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
                -deltaHeight * droplet.speed * droplet.water * s.sedimentCapacityFactor,
                s.minSedimentCapacity);

            // If carrying more sediment than capacity, or if flowing uphill:
            if (droplet.sediment > sedimentCapacity || deltaHeight > 0)
            {
                var cellOffset = originalDroplet.cellOffset;

                // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                var amountToDeposit = deltaHeight > 0
                    ? Mathf.Min(deltaHeight, droplet.sediment)
                    : (droplet.sediment - sedimentCapacity) * s.depositSpeed;
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
                    (sedimentCapacity - droplet.sediment) * s.erodeSpeed,
                    -deltaHeight);

                // Use erosion brush to erode from all nodes inside the droplet's erosion radius
                var dropletIndex = originalDroplet.CalculateIndex(heightMap);
                for (var brushPointIndex = 0; brushPointIndex < erosionBrushIndices[dropletIndex].Length; brushPointIndex++)
                {
                    var nodeIndex = erosionBrushIndices[dropletIndex][brushPointIndex];
                    var weighedErodeAmount = amountToErode * erosionBrushWeights[dropletIndex][brushPointIndex];
                    var deltaSedimentIncludingHardness = weighedErodeAmount * (1f - hardnessMap.values[nodeIndex]);
                    var deltaSediment = heightMap.values[nodeIndex] < weighedErodeAmount ? heightMap.values[nodeIndex] : deltaSedimentIncludingHardness;
                    // var deltaSediment = heightMap.values[nodeIndex] < weighedErodeAmount ? heightMap.values[nodeIndex] : weighedErodeAmount;
                    heightMap.values[nodeIndex] -= deltaSediment;
                    droplet.sediment += deltaSediment;
                }
            }

            // Update droplet's speed and water content
            droplet.speed = Mathf.Sqrt(droplet.speed * droplet.speed + deltaHeight * s.gravity);
            droplet.water *= 1 - s.evaporateSpeed;
        }
    }

    void InitializeBrushIndices(int mapSize, int radius)
    {
        erosionBrushIndices = new int[mapSize * mapSize][];
        erosionBrushWeights = new float[mapSize * mapSize][];

        var xOffsets = new int[radius * radius * 4];
        var yOffsets = new int[radius * radius * 4];
        var weights = new float[radius * radius * 4];
        float weightSum = 0;
        var addIndex = 0;

        for (var i = 0; i < erosionBrushIndices.GetLength(0); i++)
        {
            var centreX = i % mapSize;
            var centreY = i / mapSize;

            if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 || centreX >= mapSize - radius)
            {
                weightSum = 0;
                addIndex = 0;
                for (var y = -radius; y <= radius; y++)
                {
                    for (var x = -radius; x <= radius; x++)
                    {
                        float sqrDst = x * x + y * y;
                        if (sqrDst < radius * radius)
                        {
                            var coordX = centreX + x;
                            var coordY = centreY + y;

                            if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                            {
                                var weight = 1 - Mathf.Sqrt(sqrDst) / radius;
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

            var numEntries = addIndex;
            erosionBrushIndices[i] = new int[numEntries];
            erosionBrushWeights[i] = new float[numEntries];

            for (var j = 0; j < numEntries; j++)
            {
                erosionBrushIndices[i][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                erosionBrushWeights[i][j] = weights[j] / weightSum;
            }
        }
    }
}
