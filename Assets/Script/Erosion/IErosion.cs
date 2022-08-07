public interface IErosion
{
    void Init(IReadableFloatField heightMap, FloatField groundMap, FloatField sedimentMap, FloatField hardnessMap, float groundToHardnessFactor,
        bool sedimentMapEnabled);

    void ErodeStep();
}
