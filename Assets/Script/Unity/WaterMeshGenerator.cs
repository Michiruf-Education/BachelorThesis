using UnityEngine;

[ExecuteAlways]
public class WaterMeshGenerator : MonoBehaviour
{
    public int width;
    public int height;

    void Start()
    {
        FloatFieldToMesh.ApplyToMeshFilter(
            GetComponent<MeshFilter>(),
            new FloatField(width, height), 0);
    }

    void OnValidate()
    {
        if (gameObject.activeInHierarchy)
            Start();
    }
}
