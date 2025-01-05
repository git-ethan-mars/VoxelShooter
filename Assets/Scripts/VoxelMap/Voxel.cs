using UnityEngine;

namespace VoxelMap
{
    public struct Voxel
    {
        public Vector3Int Position;
        public VoxelData Data;

        public Voxel(Vector3Int position, VoxelData data)
        {
            Position = position;
            Data = data;
        }
    }
}