using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ErosionBehaviour))]
[CanEditMultipleObjects]
public class ErosionBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (ErosionBehaviour) target;

        // Basic validation
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (t.mode == ErosionBehaviour.RenderMode.UpdateMesh && t.width * t.height > 65535)
        {
            EditorGUILayout.HelpBox("Meshes with more vertices than 65535 are not supported by unity.", MessageType.Error);
            EditorGUILayout.Space();
        }

        base.OnInspectorGUI();

        if (!t.enableInEditor)
            return;

        EditorGUILayout.Space();

        if (GUILayout.Button("Perform everything"))
            t.Start();

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
}
