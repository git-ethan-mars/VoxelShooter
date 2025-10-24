using System.Runtime.InteropServices;
using UnityEngine;
namespace VoxelMap
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexData
	{
		public Vector3 Position;
		public Vector3 Normal;
		public byte Red;
		public byte Green;
		public byte Blue;
		public byte AmbientOcclusion;
		public Vector2 UV;
		public float Neighbours;
	}
}