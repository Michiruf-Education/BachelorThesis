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

    [Space] //
    public List<NoiseLayer> noiseLayers;

    [Space] //
    public List<ErosionLayer> erosionLayers;

    [Header("Editor")] //
    public bool enableInEditor;
    public bool drawHeightmap;
    public bool drawOnEachErosionStep;

    // Runtime variables
    // TODO Change to float array?
    private Texture2D heightmap;

    internal void Start()
    {
        CreateHeightmap();
        ApplyNoises();
        ApplyErosion();
        Draw();
    }

    public void CreateHeightmap()
    {
        heightmap = new Texture2D(width, height, TextureFormat.RGBA64, false)
        {
            name = "Procedural Heightmap",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
    }

    public void ApplyNoises()
    {
        noiseLayers.ForEach(layer => layer.Apply(heightmap));
    }

    public void ApplyErosion(Action callbackAfterIteration = null)
    {
        erosionLayers.ForEach(layer => layer.Apply(heightmap, callbackAfterIteration));
    }

    public void Draw()
    {
        switch (mode)
        {
            case RenderMode.UpdateMesh:
                HeightmapToMesh.ApplyToMeshFilter(targetFilter, heightmap);
                break;
            case RenderMode.TesselationMaterial:
                TesselationDisplacementMaterial.Apply(targetRenderer, heightmap);
                break;
        }
    }

    void OnValidate()
    {
        if (enableInEditor)
            Start();
    }

    void OnDrawGizmosSelected()
    {
        if (drawHeightmap && heightmap)
            Graphics.DrawTexture(new Rect(0f, 0f, heightmap.width, heightmap.height), heightmap);
    }

    public enum RenderMode
    {
        UpdateMesh,
        TesselationMaterial
    }
}
