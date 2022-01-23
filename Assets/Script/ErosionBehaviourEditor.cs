using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(ErosionBehaviour))]
[CanEditMultipleObjects]
public class ErosionBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (ErosionBehaviour) target;

        // Basic validation
        if (t.mode == ErosionBehaviour.RenderMode.UpdateMesh && t.width * t.height > 65535)
        {
            EditorGUILayout.HelpBox("Meshes with more vertices than 65535 are not supported by unity.", MessageType.Error);
            EditorGUILayout.Space();
        }

        base.OnInspectorGUI();

        if (!t.enableInEditor)
            return;

        EditorGUILayout.Space();

        if (GUILayout.Button("Redraw"))
            t.Start();

        EditorGUILayout.Space();
        GUILayout.Label("Single steps", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Texture"))
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
            var timer = new Stopwatch();
            timer.Start();

            if (t.drawOnEachErosionStep)
                t.ApplyErosion(() => t.Draw());
            else
                t.ApplyErosion();
            t.Draw();

            timer.Stop();
            Debug.Log($"Erosion finished after {timer.ElapsedMilliseconds}ms)");
        }
    }
}
