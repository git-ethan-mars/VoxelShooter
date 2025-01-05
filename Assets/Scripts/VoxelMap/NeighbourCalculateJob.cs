using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
namespace VoxelMap
{
    [BurstCompile]
    public struct NeighbourCalculateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<IntPtr> ChunkPointers;
        [WriteOnly]
        public NativeArray<(IntPtr up, IntPtr down, IntPtr front, IntPtr back, IntPtr right, IntPtr left)> NeighbourPointers;
        public int ChunkCount;
        public int Height;
        public int Depth;

        public void Execute(int index)
        {
            (IntPtr up, IntPtr down, IntPtr front, IntPtr back, IntPtr right, IntPtr left) pointers =
                new ValueTuple<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>();
            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Up, index, Height, Depth,
                    ChunkCount, out var upChunkNumber))
            {
                pointers.up = ChunkPointers[upChunkNumber];
            }

            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Down, index, Height, Depth, 
                    ChunkCount, out var downChunkNumber))
            {
                pointers.down = ChunkPointers[downChunkNumber];
            }

            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Front, index, Height, Depth, 
                    ChunkCount, out var frontChunkNumber))
            {
                pointers.front = ChunkPointers[frontChunkNumber];
            }

            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Back, index, Height, Depth,
                    ChunkCount, out var backChunkNumber))
            {
                pointers.back = ChunkPointers[backChunkNumber];
            }

            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Right, index, Height, Depth, 
                    ChunkCount, out var rightChunkNumber))
            {
                pointers.right = ChunkPointers[rightChunkNumber];
            }

            if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Left, index, Height, Depth, 
                    ChunkCount, out var leftChunkNumber))
            {
                pointers.left = ChunkPointers[leftChunkNumber];
            }

            NeighbourPointers[index] = pointers;
        }
    }
}