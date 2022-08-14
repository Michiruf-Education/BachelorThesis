using UnityEngine;
using Random = System.Random;

public class HydraulicErosion : IErosion
{
    private readonly HydraulicErosionSettings s;
    private readonly Random random;

    private IReadableFloatField heightMap;
    private FloatField groundMap;
    private FloatField sedimentMap;
    private FloatField hardnessMap;
    private float groundToHardnessFactor;
    private float sedimentToSoftnessFactor;
    private bool sedimentMapEnabled;
    private float sedimentToGroundFactor;
    private Brush brush;

    public HydraulicErosion(HydraulicErosionSettings settings, int seed)
    {
        s = settings;
        random = new Random(seed);
    }

    public void Init(IReadableFloatField heightMap, FloatField groundMap, FloatField sedimentMap, FloatField hardnessMap,
        float groundToHardnessFactor, float sedimentToSoftnessFactor, bool sedimentMapEnabled, float sedimentToGroundFactor)
    {
        this.heightMap = heightMap;
        this.groundMap = groundMap;
        this.sedimentMap = sedimentMap;
        this.hardnessMap = hardnessMap;
        this.groundToHardnessFactor = groundToHardnessFactor;
        this.sedimentToSoftnessFactor = sedimentToSoftnessFactor;
        this.sedimentMapEnabled = sedimentMapEnabled;
        this.sedimentToGroundFactor = sedimentToGroundFactor;
        brush = new Brush(heightMap.width, heightMap.height, s.erosionRadius);
    }

    public void ErodeStep()
    {
        // Create water droplet at random point on map
        var droplet = new Droplet(
            random.NextFloat(0, heightMap.width - 1),
            random.NextFloat(0, heightMap.height - 1),
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
                // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                var amountToDeposit = deltaHeight > 0
                    ? Mathf.Min(deltaHeight, droplet.sediment)
                    : (droplet.sediment - sedimentCapacity) * s.depositSpeed;
                droplet.sediment -= amountToDeposit;

                // Add the sediment to the four nodes of the current cell using bilinear interpolation
                // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                var dropletIndex = originalDroplet.CalculateIndex(heightMap);
                var cellOffset = originalDroplet.cellOffset;

                // Take some sediment and put in on the ground
                if (sedimentMapEnabled && sedimentToGroundFactor != 0f)
                {
                    var inverseFactor = 1f - sedimentToGroundFactor;
                    var previousSediment00 = sedimentMap[dropletIndex];
                    var previousSediment01 = sedimentMap[dropletIndex + 1];
                    var previousSediment10 = sedimentMap[dropletIndex + heightMap.width];
                    var previousSediment11 = sedimentMap[dropletIndex + heightMap.width + 1];
                    sedimentMap[dropletIndex] = previousSediment00 * inverseFactor;
                    sedimentMap[dropletIndex + 1] = previousSediment01 * inverseFactor;
                    sedimentMap[dropletIndex + heightMap.width] = previousSediment10 * inverseFactor;
                    sedimentMap[dropletIndex + heightMap.width + 1] = previousSediment11 * inverseFactor;
                    groundMap[dropletIndex] += previousSediment00 * sedimentToGroundFactor;
                    groundMap[dropletIndex + 1] += previousSediment01 * sedimentToGroundFactor;
                    groundMap[dropletIndex + heightMap.width] += previousSediment10 * sedimentToGroundFactor;
                    groundMap[dropletIndex + heightMap.width + 1] += previousSediment11 * sedimentToGroundFactor;
                }

                var targetMap = sedimentMapEnabled ? sedimentMap : groundMap;
                // NOTE Introducing this variable here produces a slightly different result because of floating point precision
                var amount0 = amountToDeposit * (1 - cellOffset.x) * (1 - cellOffset.y);
                var amount1 = amountToDeposit * cellOffset.x * (1 - cellOffset.y);
                var amount2 = amountToDeposit * (1 - cellOffset.x) * cellOffset.y;
                var amount3 = amountToDeposit * cellOffset.x * cellOffset.y;
                targetMap[dropletIndex] += amount0;
                targetMap[dropletIndex + 1] += amount1;
                targetMap[dropletIndex + heightMap.width] += amount2;
                targetMap[dropletIndex + heightMap.width + 1] += amount3;

                // Soften the hardness map if its enabled
                if (sedimentToSoftnessFactor != 0f)
                {
                    hardnessMap[dropletIndex] -= amount0 * sedimentToSoftnessFactor;
                    hardnessMap[dropletIndex + 1] -= amount1 * sedimentToSoftnessFactor;
                    hardnessMap[dropletIndex + heightMap.width] -= amount2 * sedimentToSoftnessFactor;
                    hardnessMap[dropletIndex + heightMap.width + 1] -= amount3 * sedimentToSoftnessFactor;
                }
            }
            else
            {
                // Erode a fraction of the droplet's current carry capacity.
                // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                var amountToErode = Mathf.Min(
                    (sedimentCapacity - droplet.sediment) * s.erodeSpeed,
                    -deltaHeight);

                // Use erosion brush to erode from all nodes inside the droplet's erosion radius
                var brushAtPosition = brush.brushMap[originalDroplet.cellPositionInt.x, originalDroplet.cellPositionInt.y];
                foreach (var brushPoint in brushAtPosition)
                {
                    var erodeAmount = amountToErode * brushPoint.weight;
                    var currentIndex = brushPoint.index;

                    // Sediment gets eroded ignoring hardness, ground uses hardness
                    // If the sediment is not enabled, then there will be always 0
                    // var removeSediment = sedimentMapEnabled ? Mathf.Min(sedimentMap[currentIndex], erodeAmount) : 0f;
                    var removeSediment = Mathf.Min(sedimentMap[currentIndex], erodeAmount);
                    var removeGround = (erodeAmount - removeSediment) * (1f - hardnessMap[currentIndex]);
                    if (!s.allowNegativeGroundValues)
                        removeGround = Mathf.Min(groundMap[currentIndex], removeGround);

                    // Update each map
                    groundMap[currentIndex] -= removeGround;
                    if (sedimentMapEnabled)
                        sedimentMap[currentIndex] -= removeSediment;
                    if (groundToHardnessFactor != 0f)
                        hardnessMap[currentIndex] += removeGround * groundToHardnessFactor;

                    // Update droplet sediment amount
                    droplet.sediment += removeSediment + removeGround;
                }
            }

            // Update droplet's speed and water content
            droplet.speed = Mathf.Sqrt(droplet.speed * droplet.speed + deltaHeight * s.gravity);
            droplet.water *= 1 - s.evaporateSpeed;
        }
    }
}
