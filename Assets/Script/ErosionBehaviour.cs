using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ErosionBehaviour : MonoBehaviour
{
    [Separator("General")] //
    [Range(1, 255)]
    public int width = 100;
    [Range(1, 255)] //
    public int height = 100;
    public float terrainHeight = 100f;

    [Space] //
    public RenderMode mode = RenderMode.UpdateMesh;
    [ConditionalField("mode", false, RenderMode.UpdateMesh)] //
    public MeshFilter targetFilter;
    [ConditionalField("mode", false, RenderMode.TesselationMaterial)] //
    public MeshRenderer targetRenderer;

    [Separator("Ground generation")] //
    [OverrideLabel("Remap ground")]
    public bool remapGroundAfterNoises = true;
    [OverrideLabel("Min")] [ConditionalField("remapGroundAfterNoises", compareValues: true)] //
    public float remapGroundAfterNoisesMin;
    [OverrideLabel("Max")] [ConditionalField("remapGroundAfterNoises", compareValues: true)] // 
    public float remapGroundAfterNoisesMax = 1f;
    [OverrideLabel("Clamp (after remap)")] //
    public bool clampGroundAfterNoises;
    public List<NoiseLayer> heightNoiseLayers;

    [Separator("Hardness generation")] //
    [OverrideLabel("Remap hardness")]
    public bool remapHardnessAfterNoises = true;
    [OverrideLabel("Min")] [ConditionalField("remapHardnessAfterNoises", compareValues: true)] //
    public float remapHardnessAfterNoisesMin;
    [OverrideLabel("Max")] [ConditionalField("remapHardnessAfterNoises", compareValues: true)] //
    public float remapHardnessAfterNoisesMax = 1f;
    [OverrideLabel("Clamp (after remap)")] //
    public bool clampHardnessAfterNoises;
    public float hardnessExponentialModifier = 1f;
    public List<NoiseLayer> hardnessNoiseLayers;

    [Separator("Sediment generation")] //
    [OverrideLabel("Remap sediment")]
    public bool remapSedimentAfterNoises = true;
    [OverrideLabel("Min")] [ConditionalField("remapSedimentAfterNoises", compareValues: true)] //
    public float remapSedimentAfterNoisesMin;
    [OverrideLabel("Max")] [ConditionalField("remapSedimentAfterNoises", compareValues: true)] // 
    public float remapSedimentAfterNoisesMax = 1f;
    [OverrideLabel("Clamp (after remap)")] //
    public bool clampSedimentAfterNoises;
    public List<NoiseLayer> sedimentNoiseLayers;

    [Separator("Erosion")] //
    [Header("Dynamic hardness")]
    public bool dynamicHardnessEnabled;
    public float groundToHardnessFactor;
    [Header("Sediment")] //
    public bool sedimentMapEnabled = true;
    public bool sedimentToGroundEnabled = true;
    [Range(0f, 1f)] // 
    public float sedimentToGroundFactor = 0.1f;
    [Header("Erosion")] //
    public bool normalizeAfterErosion = true;
    public List<ErosionLayer> erosionLayers;

    [Separator("Debug fields")] //
    public bool enableDebugFields;
    [ConditionalField("enableDebugFields", false, true)] //
    public SpriteRenderer groundMapSpriteRenderer;
    [ConditionalField("enableDebugFields", false, true)] //
    public SpriteRenderer sedimentMapSpriteRenderer;
    [ConditionalField("enableDebugFields", false, true)] //
    public SpriteRenderer hardnessMapSpriteRenderer;

    [Separator("Slow simulation")] //
    public bool slowSimulation;
    public float slowSimulationWaitTimeBetweenIterations;
    public int drawEachNthIteration = 1;
    public bool visualizeErosionStep; // TODO Implement this!

    [Separator("Editor")] //
    public bool redrawOnChange;
    public bool enableInEditor;
    public bool screenshotWireframe;
    public Camera screenshotCameraOrthographic;
    public Camera screenshotCameraPerspective;
    public Camera screenshotCameraOverview;

    // Runtime variables
    internal CompoundFloatField heightMap;
    internal FloatField groundMap;
    internal FloatField sedimentMap;
    internal FloatField hardnessMap;

    void Start()
    {
        DoEverything();
    }

    public void DoEverything()
    {
        CreateData();
        ApplyNoises();
        ApplyErosion();
        Draw();
    }

    public void CreateData()
    {
        groundMap = new FloatField(width, height);
        sedimentMap = new FloatField(width, height);
        hardnessMap = new FloatField(width, height);
        heightMap = new CompoundFloatField(BlendMode.Add, groundMap, sedimentMap);
    }

    public void ApplyNoises()
    {
        var timer = new Stopwatch();
        timer.Start();

        heightNoiseLayers.ForEach(layer => layer.Apply(groundMap));
        sedimentNoiseLayers.ForEach(layer => layer.Apply(sedimentMap));
        hardnessNoiseLayers.ForEach(layer => layer.Apply(hardnessMap));

        // Remap height
        if (remapGroundAfterNoises)
            groundMap.Remap(remapGroundAfterNoisesMin, remapGroundAfterNoisesMax);
        if (clampGroundAfterNoises)
            groundMap.ChangeAll(Mathf.Clamp01);

        // Remap sediment
        if (remapSedimentAfterNoises)
            sedimentMap.Remap(remapSedimentAfterNoisesMin, remapSedimentAfterNoisesMax);
        if (clampSedimentAfterNoises)
            sedimentMap.ChangeAll(Mathf.Clamp01);

        // Remap hardness
        if (remapHardnessAfterNoises)
            hardnessMap.Remap(remapHardnessAfterNoisesMin, remapHardnessAfterNoisesMax);
        if (clampHardnessAfterNoises)
            hardnessMap.ChangeAll(Mathf.Clamp01);

        // Apply exponential factor to hardness
        hardnessMap.ChangeAll(f => Mathf.Pow(f, hardnessExponentialModifier));

        // NOTE This is to get the values for a map (rotated in Lyx by -90 deg)
        // Usage: Make a newline after each row (determined by the size) and put them one by one in Lyx
        // var groundMapStringForLyx = groundMap.values
        //     // Following line is correct without rotation!
        //     //.Select((f, i) => string.Format("[{2},{1}] => {0:N2}   ", f, groundMap.GetXFromIndex(i), height - 1 - groundMap.GetYFromIndex(i)))
        //     .Select((f, i) => string.Format("[{1},{2}] => {0:N0}   ", f * 10f, groundMap.GetXFromIndex(i), groundMap.GetYFromIndex(i)))
        //     .OrderBy(s => s)
        //     .Select((s, i) => s + ((i + 1) % width == 0 ? "\n" : ""))
        //     .Aggregate("", (r, t) => r + t);
        // Debug.Log(groundMapStringForLyx);

        timer.Stop();
        Debug.Log($"All noises finished after {timer.ElapsedMilliseconds}ms");
    }

    public void ApplyErosion()
    {
        var timer = new Stopwatch();
        timer.Start();

        erosionLayers.ForEach(layer => layer.Apply(this));

        timer.Stop();
        Debug.Log($"All erosion finished after {timer.ElapsedMilliseconds}ms");

        // Normalize after erosion
        if (normalizeAfterErosion)
            groundMap.Remap(0, 1);
    }

    public void Draw(bool printTimings = true)
    {
        Stopwatch timer = null;
        if (printTimings)
        {
            timer = new Stopwatch();
            timer.Start();
        }

        if (enableDebugFields)
        {
            groundMapSpriteRenderer.gameObject.SetActive(true);
            groundMapSpriteRenderer.transform.position = new Vector3(width, 0, 0);
            groundMapSpriteRenderer.sprite = groundMap.ToTexture().ToSprite();

            sedimentMapSpriteRenderer.gameObject.SetActive(true);
            sedimentMapSpriteRenderer.transform.position = new Vector3(width, 0, height);
            sedimentMapSpriteRenderer.sprite = sedimentMap.ToTexture().ToSprite();

            hardnessMapSpriteRenderer.gameObject.SetActive(true);
            hardnessMapSpriteRenderer.transform.position = new Vector3(width, 0, -height);
            hardnessMapSpriteRenderer.sprite = hardnessMap.ToTexture().ToSprite();
        }
        else
        {
            groundMapSpriteRenderer.gameObject.SetActive(false);
            sedimentMapSpriteRenderer.gameObject.SetActive(false);
            hardnessMapSpriteRenderer.gameObject.SetActive(false);
        }

        switch (mode)
        {
            case RenderMode.UpdateMesh:
                FloatFieldToMesh.ApplyToMeshFilter(targetFilter, heightMap, terrainHeight);
                break;
            case RenderMode.TesselationMaterial:
                throw new NotImplementedException("Tesselation not yet implemented.");
        }

        if (timer != null)
        {
            timer.Stop();
            Debug.Log($"All drawing finished after {timer.ElapsedMilliseconds}ms");
        }
    }

    void OnValidate()
    {
        if (!redrawOnChange)
            return;
        CreateData();
        ApplyNoises();
        // Do not do erosion here, because it may be way to expansive with enough iterations
        Draw();
    }

    public enum RenderMode
    {
        UpdateMesh,
        TesselationMaterial
    }
}
