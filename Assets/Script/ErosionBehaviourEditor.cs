using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ErosionBehaviour))]
[CanEditMultipleObjects]
public class ErosionBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (ErosionBehaviour)target;

        // Basic validation
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (t.mode == ErosionBehaviour.RenderMode.UpdateMesh && t.width * t.height > 65535)
        {
            EditorGUILayout.HelpBox("Meshes with more vertices than 65535 are not supported by unity.", MessageType.Error);
            EditorGUILayout.Space();
        }

        base.OnInspectorGUI();

        if (t.enableInEditor)
            DrawEditorControls(t);

        DrawSaveDataControls(t);
    }

    private void DrawEditorControls(ErosionBehaviour t)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        if (GUILayout.Button("Perform everything"))
            t.DoEverything();

        EditorGUILayout.Space();
        GUILayout.Label("Single steps (including draw on each step)", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Data"))
        {
            t.CreateData();
            t.Draw();
        }

        if (GUILayout.Button("Apply Noises"))
        {
            t.ApplyNoises();
            t.Draw();
        }

        if (GUILayout.Button("Erode"))
        {
            t.ApplyErosion();
            t.Draw();
        }

        if (GUILayout.Button("Draw only"))
            t.Draw();
    }

    private void DrawSaveDataControls(ErosionBehaviour t)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        GUILayout.Label("Export data", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        var now = DateTime.Now;

        if (GUILayout.Button("Save base data"))
        {
            GL.wireframe = t.screenshotWireframe;
            SaveTexture(t.groundMap, t, now, "ground", false);
            SaveTexture(t.hardnessMap, t, now, "hardness", false);
            SaveTexture(t.sedimentMap, t, now, "sediment", false);
            SaveScreenshot(t.screenshotCameraOrthographic, t, now, "terrain_orthographic", false);
            SaveScreenshot(t.screenshotCameraPerspective, t, now, "terrain_perspective", false);
            SaveScreenshot(t.screenshotCameraOverview, t, now, "terrain_overview", false);
            SaveUnityData(t, now);
            GL.wireframe = false;
        }

        if (GUILayout.Button("Save eroded data"))
        {
            GL.wireframe = t.screenshotWireframe;
            SaveTexture(t.groundMap, t, now, "ground", true);
            SaveTexture(t.hardnessMap, t, now, "hardness", true);
            SaveTexture(t.sedimentMap, t, now, "sediment", true);
            SaveScreenshot(t.screenshotCameraOrthographic, t, now, "terrain_orthographic", true);
            SaveScreenshot(t.screenshotCameraPerspective, t, now, "terrain_perspective", true);
            SaveScreenshot(t.screenshotCameraOverview, t, now, "terrain_overview", true);
            SaveUnityData(t, now);
            GL.wireframe = false;
        }
    }

    private void SaveTexture(IReadableFloatField field, ErosionBehaviour t, DateTime dateTime, string basename, bool eroded)
    {
        var texture = field.ToTexture();
        SaveTexture(texture, t, dateTime, basename, eroded);
        DestroyImmediate(texture);
    }

    private void SaveTexture(Texture2D texture, ErosionBehaviour t, DateTime dateTime, string basename, bool eroded)
    {
        var data = texture.EncodeToPNG();

        var sb = new StringBuilder();
        sb.Append(Application.dataPath).Append("/DOC/Screenshots/");
        sb.Append(dateTime.ToString("yyyy-MM-dd HH-mm-ss")).Append("/");
        sb.Append(basename);

        if (eroded)
        {
            var iterations = t.erosionLayers[0].iterations;
            sb.Append("_").Append("erosion-").Append(iterations);

            if (t.groundToHardnessEnabled)
                sb.Append("_").Append("groundToHardness-").Append(t.groundToHardnessFactor);

            if (t.sedimentToSoftnessEnabled)
                sb.Append("_").Append("sedimentToSoftness-").Append(t.sedimentToSoftnessFactor);

            if (t.sedimentMapEnabled)
                sb.Append("_sediment");

            if (t.sedimentToGroundEnabled)
                sb.Append("_sedimentToGround-").Append(t.sedimentToGroundFactor);
        }

        var path = sb.Append(".png").ToString();

        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        File.WriteAllBytes(path, data);
        Debug.Log($"Saved texture to {path}");
    }

    private void SaveScreenshot(Camera camera, ErosionBehaviour t, DateTime dateTime, string basename, bool eroded)
    {
        var rect = new RectInt(0, 0, 1920, 1080);

        var previousCameraTargetTexture = camera.targetTexture;
        var previousRenderTextureActive = RenderTexture.active;

        // Render the camera into a render texture
        var renderTexture = new RenderTexture(rect.width, rect.height, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        // Read from the render texture into the texture2D
        var screenshot = new Texture2D(rect.width, rect.height, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, rect.width, rect.height), 0, 0);

        // Reset data
        camera.targetTexture = previousCameraTargetTexture;
        RenderTexture.active = previousRenderTextureActive;

        // Save
        SaveTexture(screenshot, t, dateTime, basename, eroded);

        // Clear for GC
        DestroyImmediate(renderTexture);
        DestroyImmediate(screenshot);
    }

    private void SaveUnityData(ErosionBehaviour t, DateTime dateTime)
    {
        var sb = new StringBuilder();
        sb.Append(Application.dataPath).Append("/DOC/Screenshots/");
        sb.Append(dateTime.ToString("yyyy-MM-dd HH-mm-ss")).Append("/");

        var path = sb.Append("settings.json").ToString();

        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        var json = JsonUtility.ToJson(t, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved settings to {path}");
    }
}
