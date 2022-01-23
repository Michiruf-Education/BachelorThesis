using System;
using MyBox;
using UnityEngine;

[Serializable]
public class ErosionLayer
{
    public bool enabled = true;
    public ErosionType type;
    public int seed;

    [ConditionalField("type", false, ErosionType.Hydraulic)]
    public HydraulicErosionSettings hydraulicErosionSettings;

    public void Apply(Texture2D heightmap, Action callbackAfterIteration)
    {
        if (!enabled)
            return;

        switch (type)
        {
            case ErosionType.Hydraulic:
                Apply(new HydraulicErosion(), heightmap, seed, callbackAfterIteration);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void Apply(IErosion erosion, Texture2D tex, int seed, Action callbackAfterIteration)
    {
        var pixels = new Color[tex.width * tex.height];
        var floatSeed = (float) seed;

        for (var y = 0f; y < tex.height; y++)
        {
            for (var x = 0f; x < tex.width; x++)
            {
                // TODO
            }
        }

        // Copy the pixel data to the texture and load it into the GPU
        tex.SetPixels(pixels);
        tex.Apply();
    }

    public enum ErosionType
    {
        Hydraulic,
        Wind,
        Thermal
    }
}
