using System.Runtime.InteropServices;
using UnityEngine;
namespace VoxelMap
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexData
	{
		public byte AO => (byte)(Color >> 24 & 255);
		
		public Vector3 Position;
		public Vector3 Normal;
		public uint Color;
		public Vector2 UV;
		public float Neighbours;
	}
}