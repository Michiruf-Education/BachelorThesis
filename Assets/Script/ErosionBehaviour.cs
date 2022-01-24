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

    [Separator("Noises")] //
    public bool normalizeNoises = true;
    public List<NoiseLayer> heightNoiseLayers;
    public List<NoiseLayer> hardnessNoiseLayers;

    [Separator("Erosion")] //
    public List<ErosionLayer> erosionLayers;

    [Separator("Debug fields")] //
    public bool enableDebugFields;
    [ConditionalField("enableDebugFields", false, true)]
    public SpriteRenderer heightMapSpriteRenderer;
    [ConditionalField("enableDebugFields", false, true)]
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
    private FloatField heightMap;
    private FloatField hardnessMap;

    internal void Start()
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
        hardnessNoiseLayers.ForEach(layer => layer.Apply(hardnessMap));

        // Normalize / remap those maps
        if (normalizeNoises)
        {
            heightMap.Remap(0, 1);
            hardnessMap.Remap(0, 1);
        }

        timer.Stop();
        Debug.Log($"All noises finished after {timer.ElapsedMilliseconds}ms");
    }

    public void ApplyErosion()
    {
        var timer = new Stopwatch();
        timer.Start();

        erosionLayers.ForEach(layer => layer.Apply(heightMap, hardnessMap, this));

        timer.Stop();
        Debug.Log($"All erosion finished after {timer.ElapsedMilliseconds}ms");
    }

    public void Draw(bool printTimings = true)
    {
        Stopwatch timer = null;
        if (printTimings)
        {
            timer = new Stopwatch();
            timer.Start();
        }

        var heightMapTexture = heightMap.ToTexture();

        if (enableDebugFields)
        {
            heightMapSpriteRenderer.gameObject.SetActive(true);
            heightMapSpriteRenderer.transform.position = new Vector3(width, 0, 0);
            hardnessMapSpriteRenderer.gameObject.SetActive(true);
            hardnessMapSpriteRenderer.transform.position = new Vector3(width, 0, -height);
            if (heightMapSpriteRenderer)
                heightMapSpriteRenderer.sprite = heightMapTexture.ToSprite();
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
                HeightmapToMesh.ApplyToMeshFilter(targetFilter, heightMap, terrainHeight);
                break;
            case RenderMode.TesselationMaterial:
                TesselationDisplacementMaterial.Apply(targetRenderer, heightMapTexture);
                break;
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
