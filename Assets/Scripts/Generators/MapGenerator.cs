using Data;
using Infrastructure.Factory;
using MapLogic;
using Networking;
using Optimization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Generators
{
    public class MapGenerator
    {
        private const string ChunkContainerName = "ChunkContainer";
        private const string WallContainerName = "WallContainer";
        public ChunkMeshGenerator[] ChunksGenerators { get; private set; }
        private readonly IMapProvider _mapProvider;
        private readonly IMeshFactory _meshFactory;
        private readonly GameObject _chunkContainer;

        public MapGenerator(Client client)
        {
            _mapProvider = client.MapProvider;
            _meshFactory = client.MeshFactory;
            _chunkContainer = client.GameFactory.CreateGameObjectContainer(ChunkContainerName);
            CreateChunkGenerators();
            SetNeighbours();
            InitializeChunkData();
            var wallContainer = client.GameFactory.CreateGameObjectContainer(WallContainerName);
            client.MeshFactory.CreateWalls(_mapProvider, wallContainer.transform);
        }

        private void InitializeChunkData()
        {
            var dataToDispose1 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var dataToDispose2 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var dataToDispose3 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var dataToDispose4 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var dataToDispose5 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var dataToDispose6 = new NativeArray<BlockData>[ChunksGenerators.Length];
            var jobHandlesList = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < ChunksGenerators.Length; i++)
            {
                var frontNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var backNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var upperNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var lowerNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var rightNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                var leftNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.TempJob);
                if (i + 1 < ChunksGenerators.Length &&
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
                                ChunksGenerators[i + 1].ChunkData
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
                                ChunksGenerators[i - 1].ChunkData.Blocks[
                                    x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1];
                        }
                    }
                }

                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < ChunksGenerators.Length &&
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
                                ChunksGenerators[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
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
                                ChunksGenerators[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[
                                        x * ChunkData.ChunkSizeSquared +
                                        (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < ChunksGenerators.Length)
                {
                    rightNeighbourBlocks.Dispose();
                    rightNeighbourBlocks =
                        new NativeArray<BlockData>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            rightNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                ChunksGenerators[
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
                                ChunksGenerators[
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

            for (var i = 0; i < ChunksGenerators.Length; i++)
            {
                var chunk = ChunksGenerators[i];
                var jobHandle = new ChunkMeshGeneration()
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
            for (var i = 0; i < ChunksGenerators.Length; i++)
            {
                dataToDispose1[i].Dispose();
                dataToDispose2[i].Dispose();
                dataToDispose3[i].Dispose();
                dataToDispose4[i].Dispose();
                dataToDispose5[i].Dispose();
                dataToDispose6[i].Dispose();
                ChunksGenerators[i].ApplyMesh();
            }
        }

        private void SetNeighbours()
        {
            for (var i = 0; i < ChunksGenerators.Length; i++)
            {
                if (i + 1 < ChunksGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    ChunksGenerators[i].FrontNeighbour = ChunksGenerators[i + 1];
                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    ChunksGenerators[i].BackNeighbour = ChunksGenerators[i - 1];
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < ChunksGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    ChunksGenerators[i].UpperNeighbour =
                        ChunksGenerators[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    ChunksGenerators[i].LowerNeighbour =
                        ChunksGenerators[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < ChunksGenerators.Length)
                    ChunksGenerators[i].RightNeighbour =
                        ChunksGenerators[
                            i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                    ChunksGenerators[i].LeftNeighbour =
                        ChunksGenerators[
                            i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
            }
        }

        private void CreateChunkGenerators()
        {
            ChunksGenerators = new ChunkMeshGenerator[_mapProvider.MapData.Width / ChunkData.ChunkSize *
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
                        var chunkMeshRenderer = _meshFactory.CreateChunkMeshRender(new Vector3Int(
                            x * ChunkData.ChunkSize,
                            y * ChunkData.ChunkSize,
                            z * ChunkData.ChunkSize), Quaternion.identity, _chunkContainer.transform);
                        ChunksGenerators[index] = new ChunkMeshGenerator(chunkMeshRenderer);
                        ChunksGenerators[index].ChunkData = _mapProvider.MapData.Chunks[index];
                    }
                }
            }
        }
    }
}