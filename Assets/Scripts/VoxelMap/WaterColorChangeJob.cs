using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
    [BurstCompile]
    public struct WaterColorChangeJob : IJob
    {
        public NativeArray<VoxelData> Voxels;
        public Color32 WaterColor;
        public void Execute()
        {
            for (var x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (var z = 0; z < ChunkData.ChunkSize; z++)
                {
                    var index =  x * ChunkData.ChunkSizeSquared + z;
                    Voxels[index] = new VoxelData(WaterColor);
                }
            }
        }
    }
}