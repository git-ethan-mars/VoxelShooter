using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data
{
    public class MeshData
    {
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color32> Colors;
        public readonly List<Vector3> Normals;
        public readonly IndexFormat IndexFormat;

        public MeshData(List<Vector3> vertices, List<int> triangles, List<Color32> colors, List<Vector3> normals)
        {
            Vertices = vertices;
            Triangles = triangles;
            Colors = colors;
            Normals = normals;
            IndexFormat = Vertices.Count < 65536 ? IndexFormat.UInt16 : IndexFormat.UInt32;
        }
    }
}