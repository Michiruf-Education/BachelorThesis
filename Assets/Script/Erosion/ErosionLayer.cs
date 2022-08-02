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

    [ConditionalField("type", false, ErosionType.Hydraulic)] //
    public HydraulicErosionSettings hydraulicErosionSettings;
    [ConditionalField("type", false, ErosionType.Wind)] //
    public WindErosionSettings windErosionSettings;

    public void Apply(ErosionBehaviour b)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case ErosionType.Hydraulic:
                Erode(new HydraulicErosion(hydraulicErosionSettings, seed), b);
                break;
            case ErosionType.Wind:
                Erode(new WindErosion(windErosionSettings, seed), b);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Erode(IErosion erosion, ErosionBehaviour b)
    {
        var timer = new Stopwatch();
        timer.Start();

        erosion.Init(
            b.heightMap,
            b.groundMap,
            b.sedimentMap,
            b.hardnessMap,
            b.dynamicHardnessEnabled ? b.heightToHardnessFactor : 0f
        );

        if (b.slowSimulation)
            MyCoroutineHandler.StartCoroutine(ErodeAsync(erosion, b), b, this);
        else
            ErodeSync(erosion);

        timer.Stop();
        Debug.Log($"{erosion.GetType().Name} finished after {timer.ElapsedMilliseconds}ms");
    }

    private void ErodeSync(IErosion erosion)
    {
        for (var i = 0; i < iterations; i++)
        {
            erosion.ErodeStep();
        }
    }

    private IEnumerator ErodeAsync(IErosion erosion, ErosionBehaviour b)
    {
        for (var i = 0; i < iterations; i++)
        {
            erosion.ErodeStep();

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
