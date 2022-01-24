public class MyCustomNoise : INoise
{
    public float Sample(float x, float y)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        var sample = noise.GetNoise(x, y);
        sample += 1f;
        sample /= 2f;
        return sample;
    }
}
