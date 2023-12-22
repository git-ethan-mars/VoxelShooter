using System;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Infrastructure.Factory;
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
        private const string WallContainerName = "WallContainer";
        private const float WaterScale = 2.0f;

        private readonly MapProvider _mapProvider;
        private readonly IMeshFactory _meshFactory;
        private readonly IGameFactory _gameFactory;

        public MapGenerator(MapProvider mapProvider, IGameFactory gameFactory, IMeshFactory meshFactory)
        {
            _mapProvider = mapProvider;
            _gameFactory = gameFactory;
            _meshFactory = meshFactory;
        }

        public unsafe ChunkMesh[] GenerateMap(Transform parent)
        {
            var handles = new ulong[_mapProvider.GetChunkCount()];
            var addresses = new void*[_mapProvider.GetChunkCount()];

            for (var i = 0; i < addresses.Length; i++)
            {
                addresses[i] =
                    UnsafeUtility.PinGCArrayAndGetDataAddress(_mapProvider.MapData.Chunks[i].Blocks, out handles[i]);
            }

            var chunkMeshObjects = CreateChunkMeshObjects(parent);
            var jobs = InitializeJobs(addresses);
            var jobHandles = RunJobs(jobs);
            var meshes = CreateChunkMeshes(jobs, chunkMeshObjects);

            for (var i = 0; i < handles.Length; i++)
            {
                UnsafeUtility.ReleaseGCObject(handles[i]);
            }

            DisposeJobs(jobs);
            jobHandles.Dispose();
            return meshes;
        }

        public void GenerateWalls()
        {
            var wallContainer = _gameFactory.CreateGameObjectContainer(WallContainerName);
            _meshFactory.CreateWalls(_mapProvider, wallContainer);
        }

        public void GenerateWater()
        {
            _meshFactory.CreateWaterPlane(
                new Vector3((float) _mapProvider.MapData.Width / 2, 0.5f, (float) _mapProvider.MapData.Width / 2) +
                Constants.worldOffset,
                new Vector3(_mapProvider.MapData.Width * WaterScale, 1, _mapProvider.MapData.Depth * WaterScale),
                _mapProvider.MapData.WaterColor);
        }

        public void GenerateLight()
        {
            _gameFactory.CreateDirectionalLight(_mapProvider.SceneData.LightData);
        }

        private GameObject[] CreateChunkMeshObjects(Transform chunkContainer)
        {
            var chunkMeshObjects = new GameObject[_mapProvider.GetChunkCount()];
            var index = 0;
            for (var x = 0; x < _mapProvider.MapData.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < _mapProvider.MapData.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < _mapProvider.MapData.Depth / ChunkData.ChunkSize; z++)
                    {
                        chunkMeshObjects[index] = _meshFactory.CreateChunkMeshRender(
                            new Vector3(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize, z * ChunkData.ChunkSize),
                            Quaternion.identity, chunkContainer);

                        index += 1;
                    }
                }
            }

            return chunkMeshObjects;
        }

        private void SetChunkNeighbours(IReadOnlyList<ChunkMesh> meshes)
        {
            for (var i = 0; i < meshes.Count; i++)
            {
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < meshes.Count &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    meshes[i].UpperNeighbour =
                        meshes[i + _mapProvider.MapData.Depth / ChunkData.ChunkSize];

                if (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                         ChunkData.ChunkSize) ==
                    (i - _mapProvider.MapData.Depth / ChunkData.ChunkSize) /
                    (_mapProvider.MapData.Depth / ChunkData.ChunkSize * _mapProvider.MapData.Height /
                     ChunkData.ChunkSize))
                    meshes[i].LowerNeighbour =
                        meshes[i - _mapProvider.MapData.Depth / ChunkData.ChunkSize];

                if (i + 1 < meshes.Count &&
                    i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i + 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    meshes[i].FrontNeighbour = meshes[i + 1];

                if (i - 1 >= 0 && i / (_mapProvider.MapData.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (_mapProvider.MapData.Depth / ChunkData.ChunkSize))
                    meshes[i].BackNeighbour = meshes[i - 1];

                if (i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize < meshes.Count)
                    meshes[i].RightNeighbour =
                        meshes[
                            i + _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];

                if (i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                    ChunkData.ChunkSize >= 0)
                    meshes[i].LeftNeighbour =
                        meshes[
                            i - _mapProvider.MapData.Height / ChunkData.ChunkSize * _mapProvider.MapData.Depth /
                            ChunkData.ChunkSize];
            }
        }

        private unsafe ChunkMeshGenerator[] InitializeJobs(void*[] addresses)
        {
            var jobs = new ChunkMeshGenerator[addresses.Length];
            for (var i = 0; i < addresses.Length; i++)
            {
                var addressByNeighbour = new NativeParallelHashMap<int, IntPtr>(6, Allocator.TempJob);
                // order: up = 0, down = 1, front = 2, back = 3, right = 4, left = 5.
                if (i + _mapProvider.MapData.Depth / ChunkData.ChunkSize < addresses.Length &&
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

                if (i + 1 < addresses.Length &&
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
                    ChunkData.ChunkSize < addresses.Length)
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

                jobs[i] = new ChunkMeshGenerator
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

        private NativeArray<JobHandle> RunJobs(ChunkMeshGenerator[] jobs)
        {
            var jobHandles = new NativeArray<JobHandle>(jobs.Length, Allocator.Temp);
            for (var i = 0; i < jobHandles.Length; i++)
            {
                jobHandles[i] = jobs[i].Schedule();
            }

            JobHandle.CompleteAll(jobHandles);
            return jobHandles;
        }

        private ChunkMesh[] CreateChunkMeshes(ChunkMeshGenerator[] jobs, GameObject[] chunkMeshObjects)
        {
            var meshes = new ChunkMesh[chunkMeshObjects.Length];
            for (var i = 0; i < meshes.Length; i++)
            {
                _mapProvider.MapData.Chunks[i].Faces = jobs[i].Faces.ToArray();
                var meshData = new MeshData(NativeToList(jobs[i].Vertices), NativeToList(jobs[i].Triangles),
                    NativeToList(jobs[i].Colors),
                    NativeToList(jobs[i].Normals));

                meshes[i] =
                    new ChunkMesh(chunkMeshObjects[i], _mapProvider.MapData.Chunks[i], meshData);
            }

            SetChunkNeighbours(meshes);
            return meshes;
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

        private void DisposeJobs(ChunkMeshGenerator[] jobs)
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