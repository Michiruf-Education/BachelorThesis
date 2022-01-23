using System;
using UnityEngine;
using Random = System.Random;

public class HydraulicErosion : IErosion
{
    private readonly HydraulicErosionSettings settings;
    private readonly Action callbackAfterIteration;

    private readonly Random random;

    public HydraulicErosion(HydraulicErosionSettings settings, int seed)
    {
        this.settings = settings;
        random = new Random(seed);
    }

    public void Erode(FloatField height, FloatField hardness)
    {
        // // Create water droplet at random point on map
        // float posX = prng.Next(0, mapSize - 1);
        // float posY = prng.Next(0, mapSize - 1);
        // float dirX = 0;
        // float dirY = 0;
        // float speed = initialSpeed;
        // float water = initialWaterVolume;
        // float sediment = 0;
        //
        // for (int lifetime = 0; lifetime < maxDropletLifetime; lifetime++)
        // {
        //     int nodeX = (int) posX;
        //     int nodeY = (int) posY;
        //     int dropletIndex = nodeY * mapSize + nodeX;
        //     // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        //     float cellOffsetX = posX - nodeX;
        //     float cellOffsetY = posY - nodeY;
        //
        //     // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
        //     HeightAndGradient heightAndGradient = CalculateHeightAndGradient(map, mapSize, posX, posY);
        //
        //     // Update the droplet's direction and position (move position 1 unit regardless of speed)
        //     dirX = (dirX * inertia - heightAndGradient.gradientX * (1 - inertia));
        //     dirY = (dirY * inertia - heightAndGradient.gradientY * (1 - inertia));
        //     // Normalize direction
        //     float len = Mathf.Sqrt(dirX * dirX + dirY * dirY);
        //     if (len != 0)
        //     {
        //         dirX /= len;
        //         dirY /= len;
        //     }
        //     posX += dirX;
        //     posY += dirY;
        //
        //     // Stop simulating droplet if it's not moving or has flowed over edge of map
        //     if ((dirX == 0 && dirY == 0) || posX < 0 || posX >= mapSize - 1 || posY < 0 || posY >= mapSize - 1)
        //     {
        //         break;
        //     }
        //
        //     // Find the droplet's new height and calculate the deltaHeight
        //     float newHeight = CalculateHeightAndGradient(map, mapSize, posX, posY).height;
        //     float deltaHeight = newHeight - heightAndGradient.height;
        //
        //     // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
        //     float sedimentCapacity = Mathf.Max(-deltaHeight * speed * water * sedimentCapacityFactor, minSedimentCapacity);
        //
        //     // If carrying more sediment than capacity, or if flowing uphill:
        //     if (sediment > sedimentCapacity || deltaHeight > 0)
        //     {
        //         // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
        //         float amountToDeposit = (deltaHeight > 0) ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * depositSpeed;
        //         sediment -= amountToDeposit;
        //
        //         // Add the sediment to the four nodes of the current cell using bilinear interpolation
        //         // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
        //         map[dropletIndex] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
        //         map[dropletIndex + 1] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
        //         map[dropletIndex + mapSize] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
        //         map[dropletIndex + mapSize + 1] += amountToDeposit * cellOffsetX * cellOffsetY;
        //     }
        //     else
        //     {
        //         // Erode a fraction of the droplet's current carry capacity.
        //         // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
        //         float amountToErode = Mathf.Min((sedimentCapacity - sediment) * erodeSpeed, -deltaHeight);
        //
        //         // Use erosion brush to erode from all nodes inside the droplet's erosion radius
        //         for (int brushPointIndex = 0; brushPointIndex < erosionBrushIndices[dropletIndex].Length; brushPointIndex++)
        //         {
        //             int nodeIndex = erosionBrushIndices[dropletIndex][brushPointIndex];
        //             float weighedErodeAmount = amountToErode * erosionBrushWeights[dropletIndex][brushPointIndex];
        //             float deltaSediment = (map[nodeIndex] < weighedErodeAmount) ? map[nodeIndex] : weighedErodeAmount;
        //             map[nodeIndex] -= deltaSediment;
        //             sediment += deltaSediment;
        //         }
        //     }
        //
        //     // Update droplet's speed and water content
        //     speed = Mathf.Sqrt(speed * speed + deltaHeight * gravity);
        //     water *= (1 - evaporateSpeed);
        // }
    }
}
