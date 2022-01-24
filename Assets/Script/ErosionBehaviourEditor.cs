using System.Collections;
using System.Threading;
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
            // TODO Draw on each does not work at all
            if (t.drawOnEachErosionStep)
            {
                new Thread(() => { t.ApplyErosion(t.Draw); }).Start();
            }
            else
            {
                t.ApplyErosion();
                t.Draw();
            }
        }
    }
}
