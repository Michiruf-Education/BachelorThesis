public class OpenSimplex2SNoise : INoise
{
    public float Sample(float x, float y)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        var sample = noise.GetNoise(x, y);
        sample += 1f;
        sample /= 2f;
        return sample;
    }
}
