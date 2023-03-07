using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace GamePlay
{
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
            vertices.Add(new Vector3Int(0, 0, 0));
            vertices.Add(new Vector3Int(0, 0, Map.Depth - 1));
            vertices.Add(new Vector3Int(Map.Width - 1, 0, 0));
            vertices.Add(new Vector3Int(Map.Width - 1, 0, Map.Depth - 1));
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(1);
            triangles.Add(3);
            triangles.Add(2);
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}