using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class NoiseLayer
{
    public bool enabled = true;
    public NoiseType type;
    public BlendMode blendMode; // TODO Get rid of lerp?!
    [Range(0, 1)]
    public float lerp;
    public int seed;
    public float noiseScale = 1f;

    public void Apply(Texture2D heightmap)
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

    private static void Apply(INoise noise, Texture2D tex, float scale, float lerp, int seed = 0)
    {
        var pixels = new Color[tex.width * tex.height];
        var floatSeed = (float) seed;

        for (var y = 0f; y < tex.height; y++)
        {
            for (var x = 0f; x < tex.width; x++)
            {
                var sampleX = x * scale + floatSeed;
                var sampleY = y * scale + floatSeed;
                var sample = noise.Sample(sampleX, sampleY);
                var previousColor = tex.GetPixel((int) x, (int) y);
                var newColor = new Color(sample, sample, sample);
                pixels[(int) (y * tex.width + x)] = Color.Lerp(previousColor, newColor, lerp);
            }
        }

        // Copy the pixel data to the texture and load it into the GPU
        tex.SetPixels(pixels);
        tex.Apply();
    }

    public enum NoiseType
    {
        Perlin = 1,
        RigidPerlin = 100,
        Voronoi = 2,
        OpenSimplex2S = 3,
    }
}
