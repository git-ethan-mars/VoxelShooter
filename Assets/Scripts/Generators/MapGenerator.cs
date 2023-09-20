using System;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Optimization;
using Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Generators
{
    public class MapGenerator
    {
        private const string ChunkContainerName = "ChunkContainer";
        private const string WallContainerName = "WallContainer";
        public ChunkMeshGenerator[] ChunkGenerators { get; private set; }
        public GameObject ChunkContainer { get; private set; }

        private readonly MapProvider _mapProvider;
        private readonly IMeshFactory _meshFactory;
        private readonly IGameFactory _gameFactory;
        private readonly IStaticDataService _staticData;
        private GameObject _wallContainer;

        public MapGenerator(MapProvider mapProvider, IGameFactory gameFactory, IMeshFactory meshFactory,
            IStaticDataService staticData)
        {
            _mapProvider = mapProvider;
            _gameFactory = gameFactory;
            _meshFactory = meshFactory;
            _staticData = staticData;
        }

        public unsafe void GenerateMap()
        {
            ChunkContainer = _gameFactory.CreateGameObjectContainer(ChunkContainerName);
            ChunkGenerators = new ChunkMeshGenerator[_mapProvider.GetChunkCount()];
            var handles = new ulong[_mapProvider.GetChunkCount()];
            var addresses = new void*[_mapProvider.GetChunkCount()];

            for (var i = 0; i < _mapProvider.MapData.Chunks.Length; i++)
            {
                addresses[i] =
                    UnsafeUtility.PinGCArrayAndGetDataAddress(_mapProvider.MapData.Chunks[i].Blocks, out handles[i]);
            }

            var jobs = InitializeJobs(addresses);
            RunJobs(jobs);
            SetChunkNeighbours();

            for (var i = 0; i < ChunkGenerators.Length; i++)
            {
                ChunkGenerators[i].ApplyMesh();
            }

            for (var i = 0; i < handles.Length; i++)
            {
                UnsafeUtility.ReleaseGCObject(handles[i]);
            }
        }

        public void GenerateWalls()
        {
            _wallContainer = _gameFactory.CreateGameObjectContainer(WallContainerName);
            _meshFactory.CreateWalls(_mapProvider, _wallContainer.transform);
        }

        public void GenerateLight()
        {
            _gameFactory.CreateDirectionalLight(_mapProvider.SceneData.LightData);
        }

        public void ApplySkybox()
        {
            RenderSettings.skybox = _mapProvider.SceneData.Skybox;
        }

        private GameObject[] CreateChunkMeshRenders()
        {
            var chunkMeshRenderers = new GameObject[_mapProvider.GetChunkCount()];
            var index = 0;
            for (var x = 0; x < _mapProvider.MapData.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < _mapProvider.MapData.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < _mapProvider.MapData.Depth / ChunkData.ChunkSize; z++)
                    {
                        chunkMeshRenderers[index] = _meshFactory.CreateChunkMeshRender(
                            new Vector3(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize, z * ChunkData.ChunkSize),
                            Quaternion.identity, ChunkContainer.transform);

                        index += 1;
                    }
                }
            }

            return chunkMeshRenderers;
        }

        private void SetChunkNeighbours()
        {
            for (var i = 0; i < ChunkGenerators.Length; i++)
            {
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < ChunkGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    ChunkGenerators[i].UpperNeighbour =
                        ChunkGenerators[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize];

                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    ChunkGenerators[i].LowerNeighbour =
                        ChunkGenerators[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize];

                if (i + 1 < ChunkGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    ChunkGenerators[i].FrontNeighbour = ChunkGenerators[i + 1];

                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    ChunkGenerators[i].BackNeighbour = ChunkGenerators[i - 1];

                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < ChunkGenerators.Length)
                    ChunkGenerators[i].RightNeighbour =
                        ChunkGenerators[
                            i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];

                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                    ChunkGenerators[i].LeftNeighbour =
                        ChunkGenerators[
                            i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
            }
        }

        private unsafe ChunkMeshGeneration[] InitializeJobs(void*[] addresses)
        {
            var jobs = new ChunkMeshGeneration[ChunkGenerators.Length];
            for (var i = 0; i < ChunkGenerators.Length; i++)
            {
                var addressByNeighbour = new NativeParallelHashMap<int, IntPtr>(6, Allocator.TempJob);
                // order: up = 0, down = 1, front = 2, back = 3, right = 4, left = 5.
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < ChunkGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                {
                    addressByNeighbour[0] = (IntPtr) addresses[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                }
                else
                {
                    addressByNeighbour[0] = IntPtr.Zero;
                }

                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                {
                    addressByNeighbour[1] = (IntPtr) addresses[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize];
                }
                else
                {
                    addressByNeighbour[1] = IntPtr.Zero;
                }

                if (i + 1 < ChunkGenerators.Length &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                {
                    addressByNeighbour[2] = (IntPtr) addresses[i + 1];
                }
                else
                {
                    addressByNeighbour[2] = IntPtr.Zero;
                }

                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                {
                    addressByNeighbour[3] = (IntPtr) addresses[i - 1];
                }
                else
                {
                    addressByNeighbour[3] = IntPtr.Zero;
                }

                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < ChunkGenerators.Length)
                {
                    addressByNeighbour[4] = (IntPtr) addresses[i + _mapProvider.MapData.Height /
                        ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                        ChunkData.ChunkSize];
                }
                else
                {
                    addressByNeighbour[4] = IntPtr.Zero;
                }

                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                {
                    addressByNeighbour[5] = (IntPtr) addresses[i - _mapProvider.MapData.Height /
                        ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                        ChunkData.ChunkSize];
                }
                else
                {
                    addressByNeighbour[5] = IntPtr.Zero;
                }

                jobs[i] = new ChunkMeshGeneration
                {
                    Blocks = new NativeArray<BlockData>(_mapProvider.MapData.Chunks[i].Blocks, Allocator.TempJob),
                    Faces = new NativeArray<Faces>(_mapProvider.MapData.Chunks[i].Faces.Length, Allocator.TempJob),
                    Vertices = new NativeList<Vector3>(Allocator.TempJob),
                    Triangles = new NativeList<int>(Allocator.TempJob),
                    Colors = new NativeList<Color32>(Allocator.TempJob),
                    Normals = new NativeList<Vector3>(Allocator.TempJob),
                    AddressByNeighbour = addressByNeighbour,
                };
            }

            return jobs;
        }

        private void RunJobs(ChunkMeshGeneration[] jobs)
        {
            var jobHandles = new NativeArray<JobHandle>(jobs.Length, Allocator.Temp);
            for (var i = 0; i < jobHandles.Length; i++)
            {
                jobHandles[i] = jobs[i].Schedule();
            }

            JobHandle.CompleteAll(jobHandles);

            var chunkMeshRenders = CreateChunkMeshRenders();
            for (var i = 0; i < _mapProvider.GetChunkCount(); i++)
            {
                _mapProvider.MapData.Chunks[i].Faces = jobs[i].Faces.ToArray();
                var meshData = new MeshData(NativeToList(jobs[i].Vertices), NativeToList(jobs[i].Triangles),
                    NativeToList(jobs[i].Colors),
                    NativeToList(jobs[i].Normals));

                ChunkGenerators[i] =
                    new ChunkMeshGenerator(chunkMeshRenders[i], _mapProvider.MapData.Chunks[i], meshData);
            }

            DisposeJobs(jobs);
            jobHandles.Dispose();
        }

        private List<T> NativeToList<T>(NativeList<T> nativeList) where T : unmanaged
        {
            var list = new List<T>(nativeList.Length);
            for (var i = 0; i < nativeList.Length; i++)
            {
                list.Add(nativeList[i]);
            }

            return list;
        }

        private void DisposeJobs(ChunkMeshGeneration[] jobs)
        {
            for (var i = 0; i < jobs.Length; i++)
            {
                jobs[i].AddressByNeighbour.Dispose();
                jobs[i].Blocks.Dispose();
                jobs[i].Faces.Dispose();
                jobs[i].Vertices.Dispose();
                jobs[i].Triangles.Dispose();
                jobs[i].Colors.Dispose();
                jobs[i].Normals.Dispose();
            }
        }
    }
}