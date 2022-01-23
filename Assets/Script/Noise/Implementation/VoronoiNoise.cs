public class VoronoiNoise : INoise
{
    public float Sample(float x, float y)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);
        var sample = noise.GetNoise(x, y);
        sample += 1f;
        sample /= 2f;
        return sample;
    }
}
