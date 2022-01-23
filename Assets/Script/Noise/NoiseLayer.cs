using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class NoiseLayer
{
    public bool enabled = true;
    public NoiseType type;
    public BlendMode blendMode; // TODO Get rid of lerp?!
    [Range(0, 1)] public float lerp;
    public int seed;
    public float noiseScale = 1f;

    public void Apply(FloatField heightmap)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case NoiseType.Perlin:
                Apply(new PerlinNoise(), heightmap, noiseScale / 1000f, lerp, seed);
                break;
            case NoiseType.Voronoi:
                Apply(new VoronoiNoise(), heightmap, noiseScale / 10f, lerp, seed);
                break;
            case NoiseType.OpenSimplex2S:
                Apply(new OpenSimplex2SNoise(), heightmap, noiseScale / 10f, lerp, seed);
                break;
            case NoiseType.RigidPerlin:
                Apply(new RigidPerlinNoise(), heightmap, noiseScale / 1000f, lerp, seed);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void Apply(INoise noise, FloatField heightMap, float scale, float lerp, int seed = 0)
    {
        var floatSeed = (float) seed;

        for (var y = 0; y < heightMap.height; y++)
        {
            for (var x = 0; x < heightMap.width; x++)
            {
                var sampleX = x * scale + floatSeed;
                var sampleY = y * scale + floatSeed;
                var sample = noise.Sample(sampleX, sampleY);
                var previousValue = heightMap.GetValue(x, y);
                var newValue = Mathf.Lerp(previousValue, sample, lerp);
                heightMap.SetValue(x, y, newValue);
            }
        }
    }

    public enum NoiseType
    {
        Perlin = 1,
        RigidPerlin = 100,
        Voronoi = 2,
        OpenSimplex2S = 3,
    }
}
