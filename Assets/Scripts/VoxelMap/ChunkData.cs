using System;
using Unity.Collections;
namespace VoxelMap
{
    public class ChunkData : IDisposable
    {
        public const int ChunkSize = 32;
        public const int ChunkSizeSquared = 1024;
        public const int ChunkSizeCubed = 32768;
        public NativeArray<VoxelData> Voxels = new NativeArray<VoxelData>(ChunkSizeCubed, Allocator.Persistent);
        
        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkSize && y is >= 0 and < ChunkSize &&
                   z is >= 0 and < ChunkSize;
        }

        public VoxelData GetVoxel(int x, int y, int z, PositionType positionType)
        {
            if (positionType == PositionType.Global)
            {
                x %= ChunkSize;
                y %= ChunkSize;
                z %= ChunkSize;
            }

            return Voxels[x * ChunkSizeSquared + y * ChunkSize + z];
        }

        public void SetVoxel(int x, int y, int z, VoxelData data, PositionType positionType)
        {
            if (positionType == PositionType.Global)
            {
                x %= ChunkSize;
                y %= ChunkSize;
                z %= ChunkSize;
            }

            Voxels[x * ChunkSizeSquared + y * ChunkSize + z] = data;
        }
        public void Dispose()
        {
            Voxels.Dispose();
        }
    }
}