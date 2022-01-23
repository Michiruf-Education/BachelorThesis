using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ErosionBehaviour : MonoBehaviour
{
    [Header("General")] //
    public int width = 100;
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
        heightNoiseLayers.ForEach(layer => layer.Apply(heightMap));
        hardnessNoiseLayers.ForEach(layer => layer.Apply(hardnessMap));
    }

    public void ApplyErosion(Action callbackAfterIteration = null)
    {
        erosionLayers.ForEach(layer => layer.Apply(heightMap, hardnessMap, callbackAfterIteration));
    }

    public void Draw()
    {
        var heightMapTexture = heightMap.ToTexture();

        if (enableDebugFields && heightMapSpriteRenderer)
            heightMapSpriteRenderer.sprite = heightMapTexture.ToSprite();
        if (enableDebugFields && hardnessMapSpriteRenderer)
            hardnessMapSpriteRenderer.sprite = hardnessMap.ToTexture().ToSprite();

        switch (mode)
        {
            case RenderMode.UpdateMesh:
                HeightmapToMesh.ApplyToMeshFilter(targetFilter, heightMapTexture);
                break;
            case RenderMode.TesselationMaterial:
                TesselationDisplacementMaterial.Apply(targetRenderer, heightMapTexture);
                break;
        }
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
