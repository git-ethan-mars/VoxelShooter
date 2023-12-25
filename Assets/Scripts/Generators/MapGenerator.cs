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
            var handles = new ulong[_mapProvider.ChunkCount];
            var blockAddresses = new void*[_mapProvider.ChunkCount];

            for (var i = 0; i < blockAddresses.Length; i++)
            {
                blockAddresses[i] =
                    UnsafeUtility.PinGCArrayAndGetDataAddress(_mapProvider.MapData.Chunks[i].Blocks, out handles[i]);
            }

            var chunkMeshObjects = CreateChunkMeshObjects(parent);
            var jobs = InitializeJobs(blockAddresses);
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
            var chunkMeshObjects = new GameObject[_mapProvider.ChunkCount];
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
                if (_mapProvider.TryGetUpChunkNumber(i, out var upChunkNumber))
                {
                    meshes[i].UpperNeighbour = meshes[upChunkNumber];
                }

                if (_mapProvider.TryGetDownChunkNumber(i, out var downChunkNumber))
                {
                    meshes[i].LowerNeighbour = meshes[downChunkNumber];
                }

                if (_mapProvider.TryGetFrontChunkNumber(i, out var frontChunkNumber))
                {
                    meshes[i].FrontNeighbour = meshes[frontChunkNumber];
                }

                if (_mapProvider.TryGetBackChunkNumber(i, out var backChunkNumber))
                {
                    meshes[i].BackNeighbour = meshes[backChunkNumber];
                }

                if (_mapProvider.TryGetRightChunkNumber(i, out var rightChunkNumber))
                {
                    meshes[i].RightNeighbour = meshes[rightChunkNumber];
                }

                if (_mapProvider.TryGetLeftChunkNumber(i, out var leftChunkNumber))
                {
                    meshes[i].LeftNeighbour = meshes[leftChunkNumber];
                }
            }
        }

        private unsafe ChunkMeshGenerator[] InitializeJobs(void*[] blockAddresses)
        {
            var jobs = new ChunkMeshGenerator[blockAddresses.Length];
            for (var i = 0; i < blockAddresses.Length; i++)
            {
                var addressByNeighbour = new NativeArray<IntPtr>(6, Allocator.TempJob);
                // order: up = 0, down = 1, front = 2, back = 3, right = 4, left = 5.
                if (_mapProvider.TryGetUpChunkNumber(i, out var upChunkNumber))
                {
                    addressByNeighbour[0] = (IntPtr) blockAddresses[upChunkNumber];
                }
                else
                {
                    addressByNeighbour[0] = IntPtr.Zero;
                }

                if (_mapProvider.TryGetDownChunkNumber(i, out var downChunkNumber))
                {
                    addressByNeighbour[1] =
                        (IntPtr) blockAddresses[downChunkNumber];
                }
                else
                {
                    addressByNeighbour[1] = IntPtr.Zero;
                }

                if (_mapProvider.TryGetFrontChunkNumber(i, out var frontChunkNumber))
                {
                    addressByNeighbour[2] = (IntPtr) blockAddresses[frontChunkNumber];
                }
                else
                {
                    addressByNeighbour[2] = IntPtr.Zero;
                }

                if (_mapProvider.TryGetBackChunkNumber(i, out var backChunkNumber))
                {
                    addressByNeighbour[3] = (IntPtr) blockAddresses[backChunkNumber];
                }
                else
                {
                    addressByNeighbour[3] = IntPtr.Zero;
                }

                if (_mapProvider.TryGetRightChunkNumber(i, out var rightChunkNumber))
                {
                    addressByNeighbour[4] = (IntPtr) blockAddresses[rightChunkNumber];
                }
                else
                {
                    addressByNeighbour[4] = IntPtr.Zero;
                }

                if (_mapProvider.TryGetLeftChunkNumber(i, out var leftChunkNumber))
                {
                    addressByNeighbour[5] = (IntPtr) blockAddresses[leftChunkNumber];
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
                    AddressByNeighbour = addressByNeighbour
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