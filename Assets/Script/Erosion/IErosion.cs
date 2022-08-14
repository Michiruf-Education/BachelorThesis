public interface IErosion
{
    void Init(IReadableFloatField heightMap, FloatField groundMap, FloatField sedimentMap, FloatField hardnessMap, 
        float groundToHardnessFactor, float sedimentToSoftnessFactor,
        bool sedimentMapEnabled, float sedimentToGroundFactor);

    void ErodeStep();
}
