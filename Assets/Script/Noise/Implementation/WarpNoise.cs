using UnityEngine;

// https://www.iquilezles.org/www/articles/warp/warp.htm
public class WarpNoise : INoise
{
    private static readonly (Vector2, Vector2)[] Constants =
    {
        (new Vector2(0f, 0f), new Vector2(5.2f, 1.3f)),
        (new Vector2(1.7f, 9.2f), new Vector2(8.3f, 2.8f)),
        (Vector2.zero, Vector2.zero)
    };
    // With a PerlinNoise, this looks a little bit more chromatic like
    private readonly INoise noise = new FbmNoise();

    public float Sample(float x, float y)
    {
        var noiseResult = Vector2.zero;
        for (var i = 0; i < Constants.Length; i++)
        {
            noiseResult = new Vector2(
                noise.Sample(x + 4 * noiseResult.x + Constants[i].Item1.x, y + 4 * noiseResult.y + Constants[i].Item1.y),
                noise.Sample(x + 4 * noiseResult.x + Constants[i].Item2.x, y + 4 * noiseResult.y + Constants[i].Item2.y)
            );
        }

        return noiseResult.x;
    }
}
