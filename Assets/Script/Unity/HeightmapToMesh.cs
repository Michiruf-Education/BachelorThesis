using System.Collections.Generic;
using UnityEngine;

// Got from https://answers.unity.com/questions/1033085/heightmap-to-mesh.html
public class HeightmapToMesh
{
    public static void CreateGameObjectForHeightmap(Texture2D heightmap)
    {
        // Create GO and add necessary components
        var go = new GameObject("ProcPlane");
        var meshFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        ApplyToMeshFilter(meshFilter, heightmap);
    }

    public static void ApplyToMeshFilter(MeshFilter meshFilter, Texture2D heightmap)
    {
        // Assign Mesh object to MeshFilter
        if (!meshFilter.sharedMesh)
            meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh = UpdateMesh(meshFilter.sharedMesh, heightmap);
    }

    public static Mesh UpdateMesh(Mesh mesh, Texture2D heightmap)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        // Bottom left section of the map, other sections are similar
        for (var y = 0; y < heightmap.height; y++)
        {
            for (var x = 0; x < heightmap.width; x++)
            {
                // Add each new vertex in the plane
                verts.Add(new Vector3(x, heightmap.GetPixel(x, y).r * 100f, y));

                // Skip if we cannot construct a triangle yet
                if (x == 0 || y == 0) continue;

                // Adds the index of the three vertices in order to make up each of the two tris
                var i = (y - 1) * heightmap.width + (x - 1);
                tris.Add(i + 1 + heightmap.width); // Bottom right
                tris.Add(i + 1); // Top right
                tris.Add(i); // Top left
                tris.Add(i); // Top left
                tris.Add(i + heightmap.width); // Bottom left
                tris.Add(i + 1 + heightmap.width); // Bottom right
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
