using System;
using System.Collections;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;

[Serializable]
public class ErosionLayer
{
    public bool enabled = true;
    public ErosionType type;
    public int seed;
    public int iterations;

    [ConditionalField("type", false, ErosionType.Hydraulic)]
    public HydraulicErosionSettings hydraulicErosionSettings;

    public void Apply(FloatField heightMap, FloatField hardnessMap, ErosionBehaviour b)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case ErosionType.Hydraulic:
                Erode(new HydraulicErosion(hydraulicErosionSettings, seed), heightMap, hardnessMap, b);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Erode(IErosion erosion, FloatField heightMap, FloatField hardnessMap, ErosionBehaviour b)
    {
        var timer = new Stopwatch();
        timer.Start();

        if (b.slowSimulation)
            MyCoroutineHandler.StartCoroutine(ErodeAsync(erosion, heightMap, hardnessMap, b), b, this);
        else
            ErodeSync(erosion, heightMap, hardnessMap, b);

        timer.Stop();
        Debug.Log($"{erosion.GetType().Name} finished after {timer.ElapsedMilliseconds}ms");
    }

    private void ErodeSync(IErosion erosion, FloatField heightMap, FloatField hardnessMap, ErosionBehaviour b)
    {
        for (var i = 0; i < iterations; i++)
        {
            erosion.ErodeStep(heightMap, hardnessMap);
        }
    }

    private IEnumerator ErodeAsync(IErosion erosion, FloatField heightMap, FloatField hardnessMap, ErosionBehaviour b)
    {
        for (var i = 0; i < iterations; i++)
        {
            erosion.ErodeStep(heightMap, hardnessMap);

            if (b.slowSimulation)
            {
                if (i % b.drawEachNthIteration == 0)
                    b.Draw(false);

                yield return new WaitForSeconds(b.slowSimulationWaitTimeBetweenIterations / 1000f);
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
