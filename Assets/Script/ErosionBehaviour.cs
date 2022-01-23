using System;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ErosionBehaviour : MonoBehaviour
{
    [Header("General")] //
    [Range(1, 255)]
    //
    public int width = 100;
    [Range(1, 255)] //
    public int height = 100;

    public RenderMode mode = RenderMode.UpdateMesh;
    [ConditionalField("mode", false, RenderMode.UpdateMesh)] //
    public MeshFilter targetFilter;
    [ConditionalField("mode", false, RenderMode.TesselationMaterial)] //
    public MeshRenderer targetRenderer;

    public bool enableDebugFields;
    [ConditionalField("enableDebugFields", false, true)]
    public SpriteRenderer heightMapSpriteRenderer;
    [ConditionalField("enableDebugFields", false, true)]
    public SpriteRenderer hardnessMapSpriteRenderer;

    [Space] //
    public bool normalizeNoises = true;
    public List<NoiseLayer> heightNoiseLayers;
    public List<NoiseLayer> hardnessNoiseLayers;

    [Space] //
    public List<ErosionLayer> erosionLayers;

    [Header("Editor")] //
    public bool enableInEditor;
    public bool drawOnEachErosionStep;

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

    public void ApplyErosion(Action callbackAfterIteration = null)
    {
        var timer = new Stopwatch();
        timer.Start();

        erosionLayers.ForEach(layer => layer.Apply(heightMap, hardnessMap, callbackAfterIteration));

        timer.Stop();
        Debug.Log($"All erosion finished after {timer.ElapsedMilliseconds}ms");
    }

    public void Draw()
    {
        var timer = new Stopwatch();
        timer.Start();

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
                HeightmapToMesh.ApplyToMeshFilter(targetFilter, heightMapTexture);
                break;
            case RenderMode.TesselationMaterial:
                TesselationDisplacementMaterial.Apply(targetRenderer, heightMapTexture);
                break;
        }

        timer.Stop();
        Debug.Log($"All drawing finished after {timer.ElapsedMilliseconds}ms");
    }

    void OnValidate()
    {
        if (enableInEditor)
            Start();
    }

    public enum RenderMode
    {
        UpdateMesh,
        TesselationMaterial
    }
}
