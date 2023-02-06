using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FloorRenderer : MonoBehaviour
{
    public Map Map { get; set; }
    private void Start()
    {
        RegenerateMesh();
    }

    private void RegenerateMesh()
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        for (var x = 0; x < Map.Width; x++)
        {
            for (var z = 0; z < Map.Depth; z++)
            {
                vertices.Add( new Vector3Int(x, 0, z));
                vertices.Add(new Vector3Int(x, 0, z+1));
                vertices.Add(new Vector3Int(x+1, 0, z));
                vertices.Add(new Vector3Int(x+1, 0, z+1));
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 1);
                triangles.Add(vertices.Count - 2);
            }
        }

        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}