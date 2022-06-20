using UnityEngine;
using Random = System.Random;

public class WindErosion : IErosion
{
    private readonly WindErosionSettings s;
    private readonly Random random;

    private FloatField heightMap;
    private FloatField sedimentMap;
    private FloatField hardnessMap;

    private WindParticleData d;
    private WindParticleData nd;

    private int index => heightMap.GetIndex(d.positionOfCell.x, d.positionOfCell.y);
    private int nIndex => heightMap.GetIndex(nd.positionOfCell.x, nd.positionOfCell.y);
    private float terrainHeight => heightMap.GetValue(d.positionOfCell.x, d.positionOfCell.y) +
                                   sedimentMap.GetValue(d.positionOfCell.x, d.positionOfCell.y);
    private float nTerrainHeight => heightMap.GetValue(nd.positionOfCell.x, nd.positionOfCell.y) +
                                    sedimentMap.GetValue(nd.positionOfCell.x, nd.positionOfCell.y);
    private bool isInBounds => heightMap.IsInBounds((int) d.position.x, (int) d.position.y);
    private bool nIsInBounds => heightMap.IsInBounds((int) nd.position.x, (int) nd.position.y);
    private Vector3 normal => heightMap.CalculateNormal(nd.positionOfCell.x, nd.positionOfCell.y); // TODO Use sediment also
    private Vector3 nNormal => heightMap.CalculateNormal(nd.positionOfCell.x, nd.positionOfCell.y); // TODO Use sediment also

    public WindErosion(WindErosionSettings settings, int seed)
    {
        s = settings;
        random = new Random(seed);
    }

    public void Init(FloatField heightMap, FloatField hardnessMap)
    {
        this.heightMap = heightMap;
        sedimentMap = new FloatField(heightMap.width, heightMap.height);
        this.hardnessMap = hardnessMap;
    }

    public void ErodeStep()
    {
        // Create wind particle at random position
        d = new WindParticleData(
            random.Next(0, heightMap.width - 1),
            random.Next(0, heightMap.height - 1)
        );

        // Perform Wind Erosion Cycle
        Handle();
    }

    private void Handle()
    {
        for (var i = 0; i < s.maxParticleLifetime; i++)
        {
            // Particles under the terrain height are moved upwards
            if (nd.height < terrainHeight)
                nd.height = terrainHeight;

            // Create different data to use both later
            nd = new WindParticleData(d);

            // Apply movement for new data
            ApplyMovement();

            // Prevent out of bounds
            if (!isInBounds || !nIsInBounds)
                break;

            // Perform the magic
            if (nd.height > terrainHeight)
                Flying();
            else
                AbrasionAndSuspension();

            // Stop when particle has equilibrium movement (no speed)
            if (nd.velocity.magnitude < 0.01)
                break;


            // Update the new data to the old one to start over again
            d = nd;
        }
    }

    private void ApplyMovement()
    {
        // When flying
        if (nd.height > nTerrainHeight)
        {
            nd.velocity.y -= s.dt * s.gravity;
        }
        // When sliding
        else
        {
            // TODO https://www.wolframalpha.com/ visualize double cross
            nd.velocity += s.dt * Vector3.Cross(Vector3.Cross(nd.velocity, nNormal), nNormal);
        }

        // Accelerate by prevailing wind
        nd.velocity += s.prevailingFactor * s.dt * (s.initialSpeed - nd.velocity);

        // Update position
        nd.position += s.dt * new Vector2(nd.velocity.x, nd.velocity.z);
        nd.height += s.dt * nd.velocity.y;
    }

    private void AbrasionAndSuspension()
    {
        var force = nd.velocity.magnitude * (terrainHeight - nd.height);

        // Abrasion
        if (sedimentMap[index] <= 0)
        {
            sedimentMap[index] = 0;
            heightMap[index] -= s.dt * s.abrasion * force * nd.sediment;
            sedimentMap[index] += s.dt * s.abrasion * force * nd.sediment;
        }

        // Suspension
        else if (sedimentMap[index] > s.dt * s.suspension * force)
        {
            sedimentMap[index] -= s.dt * s.suspension * force;
            nd.sediment += s.dt * s.suspension * force;
            Cascade(index);
        }

        // Set to zero
        else
            sedimentMap[index] = 0;
    }

    private void Flying()
    {
        nd.sediment -= s.dt * s.suspension * nd.sediment;

        sedimentMap[nIndex] += 0.5f * s.dt * s.suspension * nd.sediment;
        sedimentMap[index] += 0.5f * s.dt * s.suspension * nd.sediment;

        Cascade(nIndex);
        Cascade(index);
    }

    private void Cascade(int index)
    {
        // Neighbor Position Offsets (8-Way)
        var nx = new[]
        {
            -1, -1, -1, 0, 0, 1, 1, 1
        };
        var ny = new[]
        {
            -1, 0, 1, -1, 1, -1, 0, 1
        };

        // Neighbor Indices (8-Way
        var n = new[]
        {
            i - dim.y - 1, i - dim.y, i - dim.y + 1, i - 1, i + 1,
            i + dim.y - 1, i + dim.y, i + dim.y + 1
        };

        glm::ivec2 ipos;

        //Iterate over all Neighbors
        for (int m = 0; m < 8; m++)
        {
            ipos = pos;

            //Neighbor Out-Of-Bounds
            if (n[m] < 0 || n[m] >= heightMap.size) continue;
            if (ipos.x + nx[m] >= dim.x || ipos.y + ny[m] >= dim.y) continue;
            if (ipos.x + nx[m] < 0 || ipos.y + ny[m] < 0) continue;

            //Pile Size Difference and Excess
            float diff = (h[i] + s[i]) - (h[n[m]] + s[n[m]]);
            float excess = abs(diff) - roughness;

            //Stable Configuration
            if (excess <= 0) continue;

            float transfer;

            //Pile is Larger
            if (diff > 0)
                transfer = min(s[i], excess / 2.0);

            //Neighbor is Larger
            else
                transfer = -min(s[n[m]], excess / 2.0);

            //Perform Transfer
            s[i] -= dt * settling * transfer;
            s[n[m]] += dt * settling * transfer;
        }
    }
}
