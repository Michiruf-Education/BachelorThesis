using UnityEngine;
using Random = System.Random;

public static class RandomExtension
{
    public static float NextFloat(this Random random, float min, float max)
    {
        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }
}
