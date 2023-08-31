using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using Networking.Messages;
using Networking.Messages.Responses;
using Optimization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Rendering
{
    public class MapRenderer : MonoBehaviour
    {
        private ChunkRenderer[] _chunks;
        private IMapProvider _mapProvider;
        private IGameFactory _gameFactory;
        private List<UpdateMapResponse> _bufferToUpdateMap;

        public void Construct(IGameFactory gameFactory, IMeshFactory meshFactory, ClientData client)
        {
            _mapProvider = client.MapProvider;
            _gameFactory = gameFactory;
            _bufferToUpdateMap = client.BufferToUpdateMap;
            var fallMeshGenerator = new FallMeshGenerator(meshFactory);
            CreateChunkRenderers();
            SetNeighbours();
            var dataToDispose1 = new NativeArray<BlockData>[_chunks.Length];
            var dataToDispose2 = new NativeArray<BlockData>[_chunks.Length];
            var dataToDispose3 = new NativeArray<BlockData>[_chunks.Length];
            var dataToDispose4 = new NativeArray<BlockData>[_chunks.Length];
            var dataToDispose5 = new NativeArray<BlockData>[_chunks.Length];
            var dataToDispose6 = new NativeArray<BlockData>[_chunks.Length];
            var jobHandlesList = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < _chunks.Length; i++)
            {
                var frontNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var backNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var upperNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var lowerNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var rightNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var leftNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                if (i + 1 < _chunks.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                {
                    frontNeighbourBlocks.Dispose();
                    frontNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var y = 0; y < ChunkData.ChunkSize; y++)
                        {
                            frontNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                                _chunks[i + 1].ChunkData
                                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize];
                        }
                    }
                }

                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                {
                    backNeighbourBlocks.Dispose();
                    backNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var y = 0; y < ChunkData.ChunkSize; y++)
                        {
                            backNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                                _chunks[i - 1].ChunkData.Blocks[
                                    x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1];
                        }
                    }
                }

                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < _chunks.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
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
                                _chunks[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[x * ChunkData.ChunkSizeSquared + z];
                        }
                    }
                }

                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
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
                                _chunks[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[
                                        x * ChunkData.ChunkSizeSquared +
                                        (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < _chunks.Length)
                {
                    rightNeighbourBlocks.Dispose();
                    rightNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            rightNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                _chunks[
                                        i + _mapProvider.MapData.Height / ChunkData.ChunkSize *
                                        _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[y * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
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
                                _chunks[
                                        i - _mapProvider.MapData.Height / ChunkData.ChunkSize *
                                        _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
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

            for (var i = 0; i < _chunks.Length; i++)
            {
                var chunk = _chunks[i];
                var jobHandle = new UpdateChunkMeshJob()
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
            for (var i = 0; i < _chunks.Length; i++)
            {
                dataToDispose1[i].Dispose();
                dataToDispose2[i].Dispose();
                dataToDispose3[i].Dispose();
                dataToDispose4[i].Dispose();
                dataToDispose5[i].Dispose();
                dataToDispose6[i].Dispose();
                _chunks[i].ApplyMesh();
            }
        }

        private void SetNeighbours()
        {
            for (var i = 0; i < _chunks.Length; i++)
            {
                if (i + 1 < _chunks.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    _chunks[i].FrontNeighbour = _chunks[i + 1];
                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    _chunks[i].BackNeighbour = _chunks[i - 1];
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < _chunks.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    _chunks[i].UpperNeighbour = _chunks[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    _chunks[i].LowerNeighbour = _chunks[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < _chunks.Length)
                    _chunks[i].RightNeighbour =
                        _chunks[
                            i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                    _chunks[i].LeftNeighbour =
                        _chunks[
                            i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
            }
        }

        private void CreateChunkRenderers()
        {
            _chunks = new ChunkRenderer[_mapProvider.MapData.Width / ChunkData.ChunkSize *
                                        _mapProvider.MapData.Height /
                                        ChunkData.ChunkSize *
                                        _mapProvider.MapData.Depth /
                                        ChunkData.ChunkSize];
            for (var x = 0; x < _mapProvider.MapData.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < _mapProvider.MapData.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < _mapProvider.MapData.Depth / ChunkData.ChunkSize; z++)
                    {
                        var index = z + y * _mapProvider.MapData.Depth / ChunkData.ChunkSize +
                                    x * _mapProvider.MapData.Height / ChunkData.ChunkSize *
                                    _mapProvider.MapData.Depth /
                                    ChunkData.ChunkSize;
                        var chunkRenderer = _gameFactory.CreateChunkRenderer(new Vector3Int(x * ChunkData.ChunkSize,
                            y * ChunkData.ChunkSize,
                            z * ChunkData.ChunkSize), Quaternion.identity, transform);
                        _chunks[index] = chunkRenderer.GetComponent<ChunkRenderer>();
                        _chunks[index].ChunkData = _mapProvider.MapData.Chunks[index];
                    }
                }
            }
        }

        private void Update()
        {
            if (_bufferToUpdateMap.Count == 0) return;
            var dataByChunkIndex = new Dictionary<int, List<(Vector3Int, BlockData)>>();
            for (var i = 0; i < _bufferToUpdateMap.Count; i++)
            {
                for (var j = 0; j < _bufferToUpdateMap[i].Positions.Length; j++)
                {
                    var chunkIndex = _mapProvider.FindChunkNumberByPosition(_bufferToUpdateMap[i].Positions[j]);
                    if (!dataByChunkIndex.ContainsKey(chunkIndex))
                    {
                        dataByChunkIndex[chunkIndex] = new List<(Vector3Int, BlockData)>();
                    }

                    var localPosition = new Vector3Int(_bufferToUpdateMap[i].Positions[j].x % ChunkData.ChunkSize,
                        _bufferToUpdateMap[i].Positions[j].y % ChunkData.ChunkSize,
                        _bufferToUpdateMap[i].Positions[j].z % ChunkData.ChunkSize);
                    dataByChunkIndex[chunkIndex].Add((localPosition, _bufferToUpdateMap[i].BlockData[j]));
                }
            }

            foreach (var (chunkIndex, data) in dataByChunkIndex)
            {
                _chunks[chunkIndex].SpawnBlocks(data);
            }

            _bufferToUpdateMap.Clear();
        }
    }
}