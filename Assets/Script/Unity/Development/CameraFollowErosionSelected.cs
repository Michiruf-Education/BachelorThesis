using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CameraFollowErosionSelected : MonoBehaviour
{
    public Vector3 offset;
    public float lerp = 1f;

    private Transform target;

    void OnEnable()
    {
        if (!isActiveAndEnabled)
            return;

        SelectionChange();
        Selection.selectionChanged += SelectionChange;
        EditorApplication.update += EditorUpdate;

    }

    void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        Selection.selectionChanged -= SelectionChange;
    }

    private void SelectionChange()
    {
        if (Selection.activeTransform == null)
            return;
        
        var selection = Selection.activeTransform;
        
        var c = selection.GetComponentInChildren<ErosionBehaviour>();
        if (c != null)
        {
            target = c.transform;
            return;
        }

        var p = selection.GetComponentInParent<ErosionBehaviour>();
        if (p != null)
        {
            target = p.transform;
            return;
        }

        target = null;
    }

    private void EditorUpdate()
    {
        if (target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, target.position + offset, lerp);
    }
}
