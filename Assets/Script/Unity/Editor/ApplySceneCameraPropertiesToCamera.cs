using UnityEditor;
using UnityEngine;

public class ApplySceneCameraPropertiesToCamera
{
    [MenuItem("Tools/My/Apply Scene Camera Properties To Camera")]
    public static void Perform()
    {
        // Perform normal aligning first
        AlignToView.Perform();
        
        var target = Selection.activeTransform.GetComponent<Camera>();
        
        if (!target)
        {
            Debug.LogWarning("No camera selected.");
            return;
        }

        var sceneView = SceneView.lastActiveSceneView;
        var sceneCamera = sceneView.camera;
        target.orthographic = sceneCamera.orthographic;
        target.orthographicSize = sceneCamera.orthographicSize;
        target.fieldOfView = sceneCamera.fieldOfView;
        // NOTE There might be very much missing!
    }
}
