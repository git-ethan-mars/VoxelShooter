using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelMap
{
	public class MeshData
	{
		public readonly List<Color32> Colors;
		public readonly IndexFormat IndexFormat;
		public readonly List<Vector3> Normals;
		public readonly List<int> Triangles;
		public readonly List<Vector3> Vertices;
		public readonly List<Vector2> Uvs;

		public MeshData(List<Vector3> vertices, List<int> triangles, List<Color32> colors, List<Vector3> normals, List<Vector2> uvs)
		{
			Vertices = vertices;
			Triangles = triangles;
			Colors = colors;
			Normals = normals;
			Uvs = uvs;
			IndexFormat = Vertices.Count < 65536 ? IndexFormat.UInt16 : IndexFormat.UInt32;
		}
	}
}
