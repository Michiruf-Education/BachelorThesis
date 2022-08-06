using System.Collections.Generic;
using UnityEngine;

// Got from https://answers.unity.com/questions/1033085/heightmap-to-mesh.html
// TODO Auto generate multiple meshes
//      https://stackoverflow.com/questions/23552639/how-can-i-make-multiple-meshes-in-unity-by-script
public class FloatFieldToMesh
{
    public static void CreateGameObjectForHeightmap(IReadableFloatField floatField, float height)
    {
        // Create GO and add necessary components
        var go = new GameObject("ProcPlane");
        var meshFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        ApplyToMeshFilter(meshFilter, floatField, height);
    }

    public static void ApplyToMeshFilter(MeshFilter meshFilter, IReadableFloatField floatField, float height)
    {
        // Assign Mesh object to MeshFilter
        if (!meshFilter.sharedMesh)
            meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh = UpdateMesh(meshFilter.sharedMesh, floatField, height);
    }

    public static Mesh UpdateMesh(Mesh mesh, IReadableFloatField heightMap, float height)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        // Bottom left section of the map, other sections are similar
        for (var y = 0; y < heightMap.height; y++)
        {
            for (var x = 0; x < heightMap.width; x++)
            {
                // Add each new vertex in the plane
                verts.Add(new Vector3(x, heightMap.GetValue(x, y) * height, y));

                // Skip if we cannot construct a triangle yet
                if (x == 0 || y == 0) continue;

                // Adds the index of the three vertices in order to make up each of the two tris
                var i = (y - 1) * heightMap.width + (x - 1);
                tris.Add(i + 1 + heightMap.width); // Bottom right
                tris.Add(i + 1); // Top right
                tris.Add(i); // Top left
                tris.Add(i); // Top left
                tris.Add(i + heightMap.width); // Bottom left
                tris.Add(i + 1 + heightMap.width); // Bottom right
            }
        }

        var uvs = new Vector2[verts.Count];
        for (var i = 0; i < uvs.Length; i++)
            // Give UV coords X,Z world coords
            uvs[i] = new Vector2(verts[i].x, verts[i].z);

        // Assign verts, uvs, and tris to the mesh
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs;
        mesh.triangles = tris.ToArray();

        // Determines which way the triangles are facing
        mesh.RecalculateNormals();

        return mesh;
    }
}
