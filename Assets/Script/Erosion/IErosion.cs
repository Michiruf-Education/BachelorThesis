public interface IErosion
{
    void Init(FloatField heightMap, FloatField hardnessMap);

    void ErodeStep();
}
