using UnityEngine;

public class RigidPerlinNoise : INoise
{
    private readonly INoise perlinNoise = new PerlinNoise();

    public float Sample(float x, float y)
    {
        var sample = perlinNoise.Sample(x, y);
        sample *= 2f;
        sample -= 1f;
        sample = Mathf.Abs(sample);
        sample = Mathf.Pow(sample, 2);
        return sample;
    }
}
