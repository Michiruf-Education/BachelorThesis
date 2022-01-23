using System;
using System.Diagnostics;
using MyBox;

[Serializable]
public class ErosionLayer
{
    public bool enabled = true;
    public ErosionType type;
    public int seed;
    public int iterations;

    [ConditionalField("type", false, ErosionType.Hydraulic)]
    public HydraulicErosionSettings hydraulicErosionSettings;

    public void Apply(FloatField height, FloatField hardness, Action callbackAfterIteration)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case ErosionType.Hydraulic:
                Erode(new HydraulicErosion(hydraulicErosionSettings, seed), height, hardness, callbackAfterIteration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Erode(IErosion erosion, FloatField height, FloatField hardness, Action callbackAfterIteration)
    {
        var timer = new Stopwatch();
        timer.Start();

        for (var i = 0; i < iterations; i++)
        {
            erosion.Erode(height, hardness);
            callbackAfterIteration?.Invoke();
        }

        timer.Stop();
        UnityEngine.Debug.Log($"{erosion.GetType().Name} finished after {timer.ElapsedMilliseconds}ms");
    }

    public enum ErosionType
    {
        Hydraulic,
        Wind,
        Thermal
    }
}
