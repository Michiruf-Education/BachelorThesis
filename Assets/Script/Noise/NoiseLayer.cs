using System;

[Serializable]
public class NoiseLayer
{
    public bool enabled = true;
    public NoiseType type;
    public BlendMode blendMode;
    public int seed;
    public float noiseScale = 1f;
    public float noiseAmplitude = 1f;

    public void Apply(FloatField heightmap)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case NoiseType.Perlin:
                Apply(new PerlinNoise(), heightmap, noiseScale / 1000f);
                break;
            case NoiseType.Voronoi:
                Apply(new VoronoiNoise(), heightmap, noiseScale / 10f);
                break;
            case NoiseType.OpenSimplex2S:
                Apply(new OpenSimplex2SNoise(), heightmap, noiseScale / 10f);
                break;
            case NoiseType.RigidPerlin:
                Apply(new RigidPerlinNoise(), heightmap, noiseScale / 1000f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Apply(INoise noise, FloatField heightMap, float scale)
    {
        var floatSeed = (float) seed;

        for (var y = 0; y < heightMap.height; y++)
        {
            for (var x = 0; x < heightMap.width; x++)
            {
                var sampleX = x * scale + floatSeed;
                var sampleY = y * scale + floatSeed;
                var value = noise.Sample(sampleX, sampleY) * noiseAmplitude;
                var previousValue = heightMap.GetValue(x, y);
                var blendedValue = Blend.Calc(blendMode, previousValue, value);
                heightMap.SetValue(x, y, blendedValue);
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
