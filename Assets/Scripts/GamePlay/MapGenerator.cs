using Core;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePlay
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private ChunkRenderer chunkPrefab;
        private Map Map { get; set; }
        public ChunkRenderer[] Chunks { get; private set; }

        public void Initialize(Map map)
        {
            Map = map;
            GlobalEvents.OnSaveMapEvent.AddListener(mapName => MapWriter.SaveMap(mapName, Map));
            Chunks = new ChunkRenderer[Map.Width / ChunkData.ChunkSize * Map.Height / ChunkData.ChunkSize *
                                       Map.Depth /
                                       ChunkData.ChunkSize];
            for (var x = 0; x < Map.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < Map.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < Map.Depth / ChunkData.ChunkSize; z++)
                    {
                        var index = z + y * Map.Depth / ChunkData.ChunkSize +
                                    x * Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize;
                        var chunkRenderer = Instantiate(
                            chunkPrefab, new Vector3Int(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize,
                                z * ChunkData.ChunkSize), Quaternion.identity, transform);
                        Chunks[index] = chunkRenderer;
                        Chunks[index].ChunkData = Map.Chunks[index];
                    }
                }
            }

            for (var i = 0; i < Chunks.Length; i++)
            {
                if (i + 1 < Chunks.Length &&
                    i / (map.Depth / ChunkData.ChunkSize) == (i + 1) / (map.Depth / ChunkData.ChunkSize))
                    Chunks[i].FrontNeighbour = Chunks[i + 1];
                if (i - 1 >= 0 && i / (map.Depth / ChunkData.ChunkSize) == (i - 1) / (map.Depth / ChunkData.ChunkSize))
                    Chunks[i].BackNeighbour = Chunks[i - 1];
                if (i + Map.Depth / ChunkData.ChunkSize < Chunks.Length &&
                    i / (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize) ==
                    (i + Map.Depth / ChunkData.ChunkSize) /
                    (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize))
                    Chunks[i].UpperNeighbour = Chunks[i + Map.Depth / ChunkData.ChunkSize];
                if (i - Map.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize) ==
                    (i - Map.Depth / ChunkData.ChunkSize) /
                    (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize))
                    Chunks[i].LowerNeighbour = Chunks[i - Map.Depth / ChunkData.ChunkSize];
                if (i + Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize < Chunks.Length)
                    Chunks[i].RightNeighbour =
                        Chunks[i + Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize];
                if (i - Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize >= 0)
                    Chunks[i].LeftNeighbour =
                        Chunks[i - Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize];
            }

            var dataToDispose1 = new NativeArray<Block>[Chunks.Length];
            var dataToDispose2 = new NativeArray<Block>[Chunks.Length];
            var dataToDispose3 = new NativeArray<Block>[Chunks.Length];
            var dataToDispose4 = new NativeArray<Block>[Chunks.Length];
            var dataToDispose5 = new NativeArray<Block>[Chunks.Length];
            var dataToDispose6 = new NativeArray<Block>[Chunks.Length];
            var jobHandlesList = new NativeList<JobHandle>(Allocator.Temp);
            for (var i = 0; i < Chunks.Length; i++)
            {
                var chunk = Chunks[i];
                chunk.InitializeData();
                var frontNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                var backNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                var upperNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                var lowerNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                var rightNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                var leftNeighbourBlocks = new NativeArray<Block>(0, Allocator.TempJob);
                if (i + 1 < Chunks.Length &&
                    i / (map.Depth / ChunkData.ChunkSize) == (i + 1) / (map.Depth / ChunkData.ChunkSize))
                {
                    frontNeighbourBlocks.Dispose();
                    frontNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
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

                if (i - 1 >= 0 && i / (map.Depth / ChunkData.ChunkSize) ==
                    (i - 1) / (map.Depth / ChunkData.ChunkSize))
                {
                    backNeighbourBlocks.Dispose();
                    backNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
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

                if (i + Map.Depth / ChunkData.ChunkSize < Chunks.Length &&
                    i / (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize) ==
                    (i + Map.Depth / ChunkData.ChunkSize) /
                    (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize))
                {
                    upperNeighbourBlocks.Dispose();
                    upperNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            upperNeighbourBlocks[x * ChunkData.ChunkSize + z] =
                                Chunks[i + Map.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[x * ChunkData.ChunkSizeSquared + z];
                        }
                    }
                }

                if (i - Map.Depth / ChunkData.ChunkSize >= 0 &&
                    i / (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize) ==
                    (i - Map.Depth / ChunkData.ChunkSize) /
                    (map.Depth / ChunkData.ChunkSize * map.Height / ChunkData.ChunkSize))
                {
                    lowerNeighbourBlocks.Dispose();
                    lowerNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var x = 0; x < ChunkData.ChunkSize; x++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            lowerNeighbourBlocks[x * ChunkData.ChunkSize + z] =
                                Chunks[i - Map.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[
                                        x * ChunkData.ChunkSizeSquared +
                                        (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i + Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize < Chunks.Length)
                {
                    rightNeighbourBlocks.Dispose();
                    rightNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            rightNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                Chunks[i + Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize].ChunkData
                                    .Blocks[y * ChunkData.ChunkSize + z];
                        }
                    }
                }

                if (i - Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize >= 0)
                {
                    leftNeighbourBlocks.Dispose();
                    leftNeighbourBlocks =
                        new NativeArray<Block>(1024, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        for (var z = 0; z < ChunkData.ChunkSize; z++)
                        {
                            leftNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                                Chunks[i - Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize].ChunkData
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
    }
}