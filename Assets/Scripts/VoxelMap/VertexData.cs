using System.Runtime.InteropServices;
using UnityEngine;
namespace VoxelMap
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color32 Color;
    }

}