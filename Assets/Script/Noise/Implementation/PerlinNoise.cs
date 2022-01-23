using UnityEngine;

public class PerlinNoise : INoise
{
    public float Sample(float x, float y)
    {
        return Mathf.PerlinNoise(x, y);
    }
}
