using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Optimization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Rendering
{
    public class MapRenderer : MonoBehaviour
    {
        private ChunkRenderer[] Chunks { get; set; }
        private Map Map { get; set; }
        private IGameFactory _gameFactory;
        private Dictionary<Vector3Int, BlockData> _buffer;

        public void Construct(Map map, IGameFactory gameFactory, Dictionary<Vector3Int, BlockData> buffer)
        {
            Map = map;
            _gameFactory = gameFactory;
            _buffer = buffer;
            CreateChunkRenderers();
            SetNeighbours();
            var dataToDispose1 = new NativeArray<BlockData>[Chunks.Length];
            var dataToDispose2 = new NativeArray<BlockData>[Chunks.Length];
            var dataToDispose3 = new NativeArray<BlockData>[Chunks.Length];
            var dataToDispose4 = new NativeArray<BlockData>[Chunks.Length];
            var dataToDispose5 = new NativeArray<BlockData>[Chunks.Length];
            var dataToDispose6 = new NativeArray<BlockData>[Chunks.Length];
            var jobHandlesList = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < Chunks.Length; i++)
            {
                var frontNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var backNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var upperNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var lowerNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var rightNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var leftNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                if (i + 1 < Chunks.Length &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (Map.MapData.Depth / ChunkData.ChunkSize))
                {
                    frontNeighbourBlocks.Dispose();
                    frontNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var y = 0; y < ChunkData.ChunkSize; y++)
                        {
                            frontNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                                Chunks[i + 1].ChunkData
                                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize];
                        }
                    }
                }

                if (i - 1 >= 0 && i / (Map.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (Map.MapData.Depth / ChunkData.ChunkSize))
                {
                    backNeighbourBlocks.Dispose();
                    backNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var y = 0; y < ChunkData.ChunkSize; y++)
                        {
                            backNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                                Chunks[i - 1].ChunkData.Blocks[
                                    x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1];
                        }
                    }
                }

                if (i + Map.MapData.Depth / ChunkData.ChunkSize < Chunks.Length &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + Map.MapData.Depth / ChunkData.ChunkSize) /
                    (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                     ChunkData.ChunkSize))
                {
                    upperNeighbourBlocks.Dispose();
                    upperNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            upperNeighbourBlocks[x * ChunkData.ChunkSize + z] =
                                Chunks[i + Map.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[x * ChunkData.ChunkSizeSquared + z];
                        }
                    }
                }

                if (i - Map.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - Map.MapData.Depth / ChunkData.ChunkSize) /
                    (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                     ChunkData.ChunkSize))
                {
                    lowerNeighbourBlocks.Dispose();
                    lowerNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            lowerNeighbourBlocks[x * ChunkData.ChunkSize + z] =
                                Chunks[i - Map.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[
                                        x * ChunkData.ChunkSizeSquared +
                                        (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i + Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                    ChunkData.ChunkSize < Chunks.Length)
                {
                    rightNeighbourBlocks.Dispose();
                    rightNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            rightNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                Chunks[
                                        i + Map.MapData.Height / ChunkData.ChunkSize *
                                        Map.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[y * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i - Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                {
                    leftNeighbourBlocks.Dispose();
                    leftNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            leftNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                Chunks[
                                        i - Map.MapData.Height / ChunkData.ChunkSize *
                                        Map.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[(ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared +
                                            y * ChunkData.ChunkSize + z];
                        }
                    }
                }

                dataToDispose1[i] = frontNeighbourBlocks;
                dataToDispose2[i] = backNeighbourBlocks;
                dataToDispose3[i] = upperNeighbourBlocks;
                dataToDispose4[i] = lowerNeighbourBlocks;
                dataToDispose5[i] = rightNeighbourBlocks;
                dataToDispose6[i] = leftNeighbourBlocks;
            }

            for (var i = 0; i < Chunks.Length; i++)
            {
                var chunk = Chunks[i];
                var jobHandle = new UpdateMeshJob()
                {
                    FrontNeighbour = dataToDispose1[i], BackNeighbour = dataToDispose2[i],
                    UpperNeighbour = dataToDispose3[i], LowerNeighbour = dataToDispose4[i],
                    RightNeighbour = dataToDispose5[i], LeftNeighbour = dataToDispose6[i],
                    Blocks = chunk.ChunkData.Blocks, Faces = chunk.ChunkData.Faces, Colors = chunk.Colors,
                    Normals = chunk.Normals,
                    Triangles = chunk.Triangles, Vertices = chunk.Vertices
                }.Schedule();
                jobHandlesList.Add(jobHandle);
            }

            JobHandle.CompleteAll(jobHandlesList);
            for (var i = 0; i < Chunks.Length; i++)
            {
                dataToDispose1[i].Dispose();
                dataToDispose2[i].Dispose();
                dataToDispose3[i].Dispose();
                dataToDispose4[i].Dispose();
                dataToDispose5[i].Dispose();
                dataToDispose6[i].Dispose();
                Chunks[i].ApplyMesh();
            }
        }

        private void SetNeighbours()
        {
            for (var i = 0; i < Chunks.Length; i++)
            {
                if (i + 1 < Chunks.Length &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (Map.MapData.Depth / ChunkData.ChunkSize))
                    Chunks[i].FrontNeighbour = Chunks[i + 1];
                if (i - 1 >= 0 && i / (Map.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (Map.MapData.Depth / ChunkData.ChunkSize))
                    Chunks[i].BackNeighbour = Chunks[i - 1];
                if (i + Map.MapData.Depth / ChunkData.ChunkSize < Chunks.Length &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + Map.MapData.Depth / ChunkData.ChunkSize) /
                    (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                     ChunkData.ChunkSize))
                    Chunks[i].UpperNeighbour = Chunks[i + Map.MapData.Depth / ChunkData.ChunkSize];
                if (i - Map.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - Map.MapData.Depth / ChunkData.ChunkSize) /
                    (Map.MapData.Depth / ChunkData.ChunkSize * Map.MapData.Height /
                     ChunkData.ChunkSize))
                    Chunks[i].LowerNeighbour = Chunks[i - Map.MapData.Depth / ChunkData.ChunkSize];
                if (i + Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                    ChunkData.ChunkSize < Chunks.Length)
                    Chunks[i].RightNeighbour =
                        Chunks[
                            i + Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                            ChunkData.ChunkSize];
                if (i - Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                    Chunks[i].LeftNeighbour =
                        Chunks[
                            i - Map.MapData.Height / ChunkData.ChunkSize * Map.MapData.Depth /
                            ChunkData.ChunkSize];
            }
        }

        private void CreateChunkRenderers()
        {
            Chunks = new ChunkRenderer[Map.MapData.Width / ChunkData.ChunkSize *
                                       Map.MapData.Height /
                                       ChunkData.ChunkSize *
                                       Map.MapData.Depth /
                                       ChunkData.ChunkSize];
            for (var x = 0; x < Map.MapData.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < Map.MapData.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < Map.MapData.Depth / ChunkData.ChunkSize; z++)
                    {
                        var index = z + y * Map.MapData.Depth / ChunkData.ChunkSize +
                                    x * Map.MapData.Height / ChunkData.ChunkSize *
                                    Map.MapData.Depth /
                                    ChunkData.ChunkSize;
                        var chunkRenderer = _gameFactory.CreateChunkRenderer(new Vector3Int(x * ChunkData.ChunkSize,
                            y * ChunkData.ChunkSize,
                            z * ChunkData.ChunkSize), Quaternion.identity, transform);
                        Chunks[index] = chunkRenderer.GetComponent<ChunkRenderer>();
                        Chunks[index].ChunkData = Map.MapData.Chunks[index];
                    }
                }
            }
        }

        private void Update()
        {
            if (_buffer.Count == 0) return;
            var dataByChunkIndex = new Dictionary<int, List<(Vector3Int, BlockData)>>();
            foreach (var (globalPosition, blockData) in _buffer)
            {
                var chunkIndex = Map.FindChunkNumberByPosition(globalPosition);
                if (!dataByChunkIndex.ContainsKey(chunkIndex))
                {
                    dataByChunkIndex[chunkIndex] = new List<(Vector3Int, BlockData)>();
                }
                var localPosition = new Vector3Int(globalPosition.x % ChunkData.ChunkSize,
                    globalPosition.y % ChunkData.ChunkSize,
                    globalPosition.z % ChunkData.ChunkSize);
                dataByChunkIndex[chunkIndex].Add((localPosition, blockData));
            }

            foreach (var (chunkIndex, data) in dataByChunkIndex)
            {
                Chunks[chunkIndex].SpawnBlocks(data);
            }
            _buffer.Clear();
        }
    }
}