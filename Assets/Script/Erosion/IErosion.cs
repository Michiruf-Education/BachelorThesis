public interface IErosion
{
    void Init(FloatField heightMap, FloatField sedimentMap, FloatField hardnessMap, float heightToHardnessFactor);

    void ErodeStep();
}
