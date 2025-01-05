using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
    [BurstCompile]
    public struct InnerColorChangeJob : IJob
    {
        public NativeArray<VoxelData> Voxels;
        public Color32 NewInnerColor;
        public void Execute()
        {
            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                if (Voxels[i] == VoxelData.DefaultInner)
                {
                    Voxels[i] = new VoxelData(NewInnerColor);
                }
            }
        }
    }
}