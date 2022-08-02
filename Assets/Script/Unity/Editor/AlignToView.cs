using UnityEditor;
using UnityEngine;

public class AlignToView
{
    [MenuItem("Tools/My/Align Selection With View")]
    public static void Perform()
    {
        var target = Selection.activeTransform;

        if (!target)
        {
            Debug.LogWarning("Nothing selected.");
            return;
        }

        var sceneView = SceneView.lastActiveSceneView;
        var sceneCamera = sceneView.camera;
        var sceneCamTransform = sceneCamera.transform;
        target.position = sceneCamTransform.position;
        target.rotation = sceneCamTransform.rotation;
    }
}
