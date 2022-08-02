using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    [Separator("Height generation")] //
    [OverrideLabel("Remap height")]
    public bool remapHeightAfterNoises = true;
    [OverrideLabel("Min")] [ConditionalField("remapHeightAfterNoises", compareValues: true)] //
    public float remapHeightAfterNoisesMin;
    [OverrideLabel("Max")] [ConditionalField("remapHeightAfterNoises", compareValues: true)] // 
    public float remapHeightAfterNoisesMax = 1f;
    [OverrideLabel("Clamp (after remap)")] //
    public bool clampHeightAfterNoises;
    public List<NoiseLayer> heightNoiseLayers;

    [Separator("Sediment generation")] //
    [OverrideLabel("Remap height")]
    public bool remapSedimentAfterNoises = true;
    [OverrideLabel("Min")] [ConditionalField("remapSedimentAfterNoises", compareValues: true)] //
    public float remapSedimentAfterNoisesMin;
    [OverrideLabel("Max")] [ConditionalField("remapSedimentAfterNoises", compareValues: true)] // 
    public float remapSedimentAfterNoisesMax = 1f;
    [OverrideLabel("Clamp (after remap)")] //
    public bool clampSedimentAfterNoises;
    public List<NoiseLayer> sedimentNoiseLayers;

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

    [Separator("Dynamic hardness")] //
    public bool dynamicHardnessEnabled;
    public float heightToHardnessFactor;

    [Separator("Erosion")] //
    public bool normalizeAfterErosion = true;
    public List<ErosionLayer> erosionLayers;

    [Separator("Debug fields")] //
    public bool enableDebugFields;
    [ConditionalField("enableDebugFields", false, true)] //
    public SpriteRenderer heightMapSpriteRenderer;
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

    // Runtime variables
    internal FloatField heightMap;
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
        heightMap = new FloatField(width, height);
        hardnessMap = new FloatField(width, height);
    }

    public void ApplyNoises()
    {
        var timer = new Stopwatch();
        timer.Start();

        heightNoiseLayers.ForEach(layer => layer.Apply(heightMap));
        sedimentNoiseLayers.ForEach(layer => layer.Apply(sedimentMap));
        hardnessNoiseLayers.ForEach(layer => layer.Apply(hardnessMap));

        // Remap height
        if(remapHeightAfterNoises)
            heightMap.Remap(remapHeightAfterNoisesMin, remapHeightAfterNoisesMax);
        if(clampHeightAfterNoises)
            heightMap.ChangeAll(Mathf.Clamp01);

        // Remap sediment
        if(remapSedimentAfterNoises)
            sedimentMap.Remap(remapSedimentAfterNoisesMin, remapSedimentAfterNoisesMax);
        if(clampSedimentAfterNoises)
            sedimentMap.ChangeAll(Mathf.Clamp01);
        
        // Remap hardness
        if(remapHardnessAfterNoises)
            hardnessMap.Remap(remapHardnessAfterNoisesMin, remapHardnessAfterNoisesMax);
        if(clampHardnessAfterNoises)
            hardnessMap.ChangeAll(Mathf.Clamp01);
        
        // Apply exponential factor to hardness
        hardnessMap.ChangeAll(f => Mathf.Pow(f, hardnessExponentialModifier));

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
            heightMap.Remap(0, 1);
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
            heightMapSpriteRenderer.gameObject.SetActive(true);
            heightMapSpriteRenderer.transform.position = new Vector3(width, 0, 0);
            hardnessMapSpriteRenderer.gameObject.SetActive(true);
            hardnessMapSpriteRenderer.transform.position = new Vector3(width, 0, -height);
            if (heightMapSpriteRenderer)
                heightMapSpriteRenderer.sprite = heightMap.ToTexture().ToSprite();
            if (hardnessMapSpriteRenderer)
                hardnessMapSpriteRenderer.sprite = hardnessMap.ToTexture().ToSprite();
        }
        else
        {
            heightMapSpriteRenderer.gameObject.SetActive(false);
            hardnessMapSpriteRenderer.gameObject.SetActive(false);
        }
        
        switch (mode)
        {
            case RenderMode.UpdateMesh:
                HeightMapToMesh.ApplyToMeshFilter(targetFilter, heightMap, terrainHeight);
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
