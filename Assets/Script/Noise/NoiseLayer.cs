using System;
using UnityEngine;

[Serializable]
public class NoiseLayer
{
    public bool enabled = true;
    public NoiseType type;
    public BlendMode blendMode;
    public int seed;
    public float noiseScale = 1f;
    public float noiseAmplitude = 1f;
    public bool clampSelf;

    public void Apply(FloatField floatField)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case NoiseType.Identity:
                Apply(new IdentityNoise(), floatField, 1f);
                break;
            case NoiseType.Perlin:
                Apply(new PerlinNoise(), floatField, noiseScale / 1000f);
                break;
            case NoiseType.Voronoi:
                Apply(new VoronoiNoise(), floatField, noiseScale / 10f);
                break;
            case NoiseType.OpenSimplex2S:
                Apply(new OpenSimplex2SNoise(), floatField, noiseScale / 10f);
                break;
            case NoiseType.RigidPerlin:
                Apply(new RigidPerlinNoise(), floatField, noiseScale / 1000f);
                break;
            case NoiseType.Warp:
                Apply(new WarpNoise(), floatField, noiseScale / 1000f);
                break;
            case NoiseType.Fbm:
                Apply(new FbmNoise(), floatField, noiseScale / 1000f);
                break;
            case NoiseType.MyCustomNoise:
                Apply(new MyCustomNoise(), floatField, noiseScale / 10f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Apply(INoise noise, FloatField floatField, float scale)
    {
        var floatSeed = (float) seed;

        for (var y = 0; y < floatField.height; y++)
        {
            for (var x = 0; x < floatField.width; x++)
            {
                var sampleX = x * scale + floatSeed;
                var sampleY = y * scale + floatSeed;
                var value = noise.Sample(sampleX, sampleY) * noiseAmplitude;
                if (clampSelf)
                    value = Mathf.Clamp01(value);
                var previousValue = floatField.GetValue(x, y);
                var blendedValue = Blend.Calc(blendMode, previousValue, value);
                floatField.SetValue(x, y, blendedValue);
            }
        }
    }

    public enum NoiseType
    {
        Identity = 0,
        Perlin = 1,
        RigidPerlin = 100,
        Voronoi = 2,
        OpenSimplex2S = 3,
        Warp = 4,
        Fbm = 5,
        MyCustomNoise = 200,
    }
}
