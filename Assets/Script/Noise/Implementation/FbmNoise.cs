public class FbmNoise : INoise
{
    private const float Layers = 3;
    private readonly INoise noise = new PerlinNoise();

    public float Sample(float x, float y)
    {
        var result = 0f;
        var heightSum = 0f;

        var frequency = 1.0f;
        var amplitude = 1.0f;
        var seed = 0f;
        for (var layer = 0; layer < Layers; layer++)
        {
            result += noise.Sample(
                x * frequency + seed++,
                y * frequency + seed++
            ) * amplitude;
            heightSum += amplitude;

            frequency *= 2f;
            amplitude *= 0.5f;
        }

        return result / heightSum;
    }
}
