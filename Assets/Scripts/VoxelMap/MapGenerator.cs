using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace VoxelMap
{
    public class MapGenerator
    {
        private readonly MapData _mapData;
        private readonly IMapFactory _mapFactory;

        public MapGenerator(IMapFactory mapFactory, MapData mapData)
        {
            _mapData = mapData;
            _mapFactory = mapFactory;
        }

        public Chunk[] GenerateChunks(Transform parent)
        {
            var chunkPointers = new NativeArray<IntPtr>(_mapData.ChunkCount, Allocator.Persistent);
            unsafe
            {
                for (var i = 0; i < _mapData.ChunkCount; i++)
                {
                    chunkPointers[i] = (IntPtr)_mapData.GetChunkDataByIndex(i).Voxels.GetUnsafePtr();
                }
            }

            var neighbourPointers = new NativeArray<(IntPtr up, IntPtr down, IntPtr front, IntPtr back, IntPtr right, IntPtr left)>(
                _mapData.ChunkCount,
                Allocator.Persistent);
            var neighbourCalculateHandle = new NeighbourCalculateJob()
            {
                ChunkPointers = chunkPointers,
                ChunkCount = _mapData.ChunkCount,
                Height = _mapData.Height,
                Depth = _mapData.Depth,
                NeighbourPointers = neighbourPointers
            }.Schedule(_mapData.ChunkCount, _mapData.ChunkCount);
            neighbourCalculateHandle.Complete();
            var chunks = new Chunk[_mapData.ChunkCount];
            for (var i = 0; i < _mapData.ChunkCount; i++)
            {
                var faces = new NativeArray<Face>(ChunkData.ChunkSizeCubed, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                var calculateFaceJob = new CalculateFacesJob
                {
                    Voxels = _mapData.GetChunkDataByIndex(i).Voxels,
                    Faces = faces,
                    UpNeighbourPointer = neighbourPointers[i].up,
                    DownNeighbourPointer = neighbourPointers[i].down,
                    FrontNeighbourPointer = neighbourPointers[i].front,
                    BackNeighbourPointer = neighbourPointers[i].back,
                    RightNeighbourPointer = neighbourPointers[i].right,
                    LeftNeighbourPointer = neighbourPointers[i].left,
                }.Schedule(neighbourCalculateHandle);

                chunks[i] = _mapFactory.CreateChunk(ChunkIndexToPosition(i), parent, _mapData.GetChunkDataByIndex(i), faces);
                chunks[i].RegenerateMesh(calculateFaceJob);
            }
    
            SetChunkNeighbours(chunks);
            chunkPointers.Dispose();
            neighbourPointers.Dispose();
            
            return chunks;
        }

        private void SetChunkNeighbours(Chunk[] chunks)
        {
            for (var i = 0; i < chunks.Length; i++)
            {
                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Up, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var upChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Up] = chunks[upChunkNumber];
                }

                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Down, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var downChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Down] = chunks[downChunkNumber];
                }

                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Front, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var frontChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Front] = chunks[frontChunkNumber];
                }

                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Back, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var backChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Back] = chunks[backChunkNumber];
                }

                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Right, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var rightChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Right] = chunks[rightChunkNumber];
                }

                if (ChunkGeneratorHelper.TryGetChunkNeighbourNumber(ChunkNeighbourType.Left, i, _mapData.Height, _mapData.Depth,
                        _mapData.ChunkCount, out var leftChunkNumber))
                {
                    chunks[i].Neighbours[ChunkNeighbourType.Left] = chunks[leftChunkNumber];
                }
            }
        }

        private Vector3 ChunkIndexToPosition(int index)
        {
            var z = index % (_mapData.Depth / ChunkData.ChunkSize) * ChunkData.ChunkSize;
            var y = index / (_mapData.Depth / ChunkData.ChunkSize) % (_mapData.Height / ChunkData.ChunkSize) * ChunkData.ChunkSize;
            var x = index / (_mapData.Depth * _mapData.Height / ChunkData.ChunkSizeSquared) * ChunkData.ChunkSize;
            return new Vector3(x, y, z);
        }
    }
}