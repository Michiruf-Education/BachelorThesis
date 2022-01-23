using System;
using MyBox;

[Serializable]
public class ErosionLayer
{
    public bool enabled = true;
    public ErosionType type;
    public int seed;

    [ConditionalField("type", false, ErosionType.Hydraulic)]
    public HydraulicErosionSettings hydraulicErosionSettings;

    public void Apply(FloatField height, FloatField hardness, Action callbackAfterIteration)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case ErosionType.Hydraulic:
                Apply(new HydraulicErosion(), height, hardness, seed, callbackAfterIteration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void Apply(IErosion erosion, FloatField height, FloatField hardness, int seed, Action callbackAfterIteration)
    {
        var floatSeed = (float) seed;

        for (var y = 0f; y < height.height; y++)
        {
            for (var x = 0f; x < height.width; x++)
            {
                // TODO
            }
        }
    }

    public enum ErosionType
    {
        Hydraulic,
        Wind,
        Thermal
    }
}
