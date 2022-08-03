using MyBox;
using UnityEngine;
using Random = System.Random;

public class WindErosion : IErosion
{
    private readonly WindErosionSettings s;
    private readonly Random random;

    private IReadableFloatField heightMap;
    private FloatField groundMap;
    private FloatField sedimentMap;
    private FloatField hardnessMap;
    private float groundToHardnessFactor;

    private WindParticleData d;
    private WindParticleData nd;

    private int index => groundMap.GetIndex(d.positionOfCell.x, d.positionOfCell.y);
    private int nIndex => groundMap.GetIndex(nd.positionOfCell.x, nd.positionOfCell.y);
    private float terrainHeight => groundMap.GetValue(d.positionOfCell.x, d.positionOfCell.y) +
                                   sedimentMap.GetValue(d.positionOfCell.x, d.positionOfCell.y);
    private float nTerrainHeight => groundMap.GetValue(nd.positionOfCell.x, nd.positionOfCell.y) +
                                    sedimentMap.GetValue(nd.positionOfCell.x, nd.positionOfCell.y);
    private bool isInBounds => groundMap.IsInBounds((int)d.position.x, (int)d.position.y);
    private bool nIsInBounds => groundMap.IsInBounds((int)nd.position.x, (int)nd.position.y);

    public WindErosion(WindErosionSettings settings, int seed)
    {
        s = settings;
        random = new Random(seed);
    }

    public void Init(IReadableFloatField heightMap, FloatField groundMap, FloatField sedimentMap, FloatField hardnessMap, float groundToHardnessFactor)
    {
        this.heightMap = heightMap;
        this.groundMap = groundMap;
        this.hardnessMap = hardnessMap;
        this.sedimentMap = sedimentMap;
        this.groundToHardnessFactor = groundToHardnessFactor;
    }

    public void ErodeStep()
    {
        // Create wind particle at random position
        d = new WindParticleData(
            // TODO NextFloat!
            random.Next(0, groundMap.width - 1),
            random.Next(0, groundMap.height - 1),
            s.initialSpeed
        );

        // TODO Only used for debugging
        var c = new Color(UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value);

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
            if (nd.velocity.magnitude < 0.01f)
                break;
            
            var color = c.BrightnessOffset((float)i / s.maxParticleLifetime);
            Debug.DrawLine(new Vector3(nd.position.x, nd.height, nd.position.y), new Vector3(nd.position.x, nd.height + 20f, nd.position.y), color, 60f);

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
            // TODO What scale?
            // https://github.com/weigert/SimpleWindErosion/blob/master/source/wind.h
            var normal = SurfaceNormal(d.position, 1f);
            // TODO https://www.wolframalpha.com/ visualize double cross
            nd.velocity += s.dt * Vector3.Cross(Vector3.Cross(nd.velocity, normal), normal);
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
            groundMap[index] -= s.dt * s.abrasion * force * nd.sediment;
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
        var px = groundMap.GetXFromIndex(index);
        var py = groundMap.GetYFromIndex(index);

        // Neighbor Position Offsets (8-Way)
        var nx = new[]
        {
            -1, -1, -1, 0, 0, 1, 1, 1
        };
        var ny = new[]
        {
            -1, 0, 1, -1, 1, -1, 0, 1
        };

        // Iterate over all Neighbors
        for (var m = 0; m < 8; m++)
        {
            // Neighbor Out-Of-Bounds
            if (!groundMap.IsInBounds(px + nx[m], py + ny[m]))
                continue;

            var neighborIndex = groundMap.GetIndex(px + nx[m], py + ny[m]);

            // Pile size
            var diff = (groundMap[index] + sedimentMap[index]) - (groundMap[neighborIndex] + sedimentMap[neighborIndex]);
            var excess = Mathf.Abs(diff) - s.roughness;

            // Stable configuration
            if (excess <= 0)
                continue;

            float transfer;

            // Pile is larger
            if (diff > 0)
                transfer = Mathf.Min(sedimentMap[index], excess / 2.0f);
            // Neighbor is larger
            else
                transfer = -Mathf.Min(sedimentMap[neighborIndex], excess / 2.0f);

            // Perform Transfer
            sedimentMap[index] -= s.dt * s.settling * transfer;
            sedimentMap[neighborIndex] += s.dt * s.settling * transfer;
        }
    }

    // TODO Following up causes OOBException

    private Vector3 surfaceNormal(int index, float scale, int radius = 1)
    {
        var surface = new FloatField(groundMap.width, groundMap.height);
        surface.BlendAll(BlendMode.Add, groundMap);
        surface.BlendAll(BlendMode.Add, sedimentMap);

        // Two large triangles adjacent to the plane (+Y -> +X) (-Y -> -X)
        // If the maximum gets increased, more surrounding data will be used
        var result = Vector3.zero;
        for (var i = 1; i <= radius; i++)
        {
            var height = groundMap.height;
            result += (1f / (float)i * i) * Vector3.Cross(
                new Vector3(0f, scale * (surface[index + i] - surface[index]), i),
                new Vector3(i, scale * (surface[index + i * height] - surface[index]), 0f)
            );
            result += (1f / (float)i * i) * Vector3.Cross(
                new Vector3(0f, scale * (surface[index - i] - surface[index]), -i),
                new Vector3(-i, scale * (surface[index - i * height] - surface[index]), 0f)
            );
            result += (1f / (float)i * i) * Vector3.Cross(
                new Vector3(i, scale * (surface[index + i * height] - surface[index]), 0f),
                new Vector3(0f, scale * (surface[index - i] - surface[index]), -i)
            );
            result += (1f / (float)i * i) * Vector3.Cross(
                new Vector3(-i, scale * (surface[index - i * height] - surface[index]),
                    0f),
                new Vector3(0f, scale * (surface[index + i] - surface[index]), i)
            );
        }

        return result.normalized;
    }

    private Vector3 SurfaceNormal(Vector2 pos, float scale)
    {
        var P00 = new Vector2Int((int)pos.x, (int)pos.y); // Floored Position
        var P10 = P00 + new Vector2Int(1, 0);
        var P01 = P00 + new Vector2Int(0, 1);
        var P11 = P00 + new Vector2Int(1, 1);

        var N00 = surfaceNormal(groundMap.GetIndex(P00), scale);
        var N10 = surfaceNormal(groundMap.GetIndex(P10), scale);
        var N01 = surfaceNormal(groundMap.GetIndex(P01), scale);
        var N11 = surfaceNormal(groundMap.GetIndex(P11), scale);

        // Weights (modulo position)
        // glm::vec2 w = 1.0f-glm::mod(pos, glm::vec2(1.0));
        var w = new Vector2(1.0f - pos.x % 1.0f, 1.0f - pos.y % 1.0f);
        return w.x * w.y * N00 + (1.0f - w.x) * w.y * N10 + w.x * (1.0f - w.y) * N01 + (1.0f - w.x) * (1.0f - w.y) * N11;
    }
}
