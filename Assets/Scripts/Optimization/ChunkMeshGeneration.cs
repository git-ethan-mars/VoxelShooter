using System;
using Data;
using Generators;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Optimization
{
    [BurstCompile]
    public unsafe struct ChunkMeshGeneration : IJob
    {
        [ReadOnly]
        public NativeParallelHashMap<int, IntPtr> AddressByNeighbour;

        [ReadOnly]
        public NativeArray<BlockData> Blocks;

        public NativeArray<Faces> Faces;

        public NativeList<Vector3> Vertices;

        public NativeList<Vector3> Normals;

        public NativeList<int> Triangles;

        public NativeList<Color32> Colors;

        public void Execute()
        {
            var upperNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[0] != IntPtr.Zero)
            {
                upperNeighbourBlocks.Dispose();
                upperNeighbourBlocks = new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory);
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        upperNeighbourBlocks[x * ChunkData.ChunkSize + z] =
                            UnsafeUtility.ReadArrayElement<BlockData>(AddressByNeighbour[0].ToPointer(),
                                x * ChunkData.ChunkSizeSquared + z);
                    }
                }
            }

            var lowerNeighboursBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[1] != IntPtr.Zero)
            {
                lowerNeighboursBlocks.Dispose();
                lowerNeighboursBlocks =
                    new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        lowerNeighboursBlocks[x * ChunkData.ChunkSize + z] = UnsafeUtility.ReadArrayElement<BlockData>(
                            AddressByNeighbour[1].ToPointer(), x * ChunkData.ChunkSizeSquared +
                                                               (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z);
                    }
                }
            }

            var frontNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[2] != IntPtr.Zero)
            {
                frontNeighbourBlocks.Dispose();
                frontNeighbourBlocks =
                    new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        frontNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                            UnsafeUtility.ReadArrayElement<BlockData>(AddressByNeighbour[2].ToPointer(),
                                x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize);
                    }
                }
            }

            var backNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[3] != IntPtr.Zero)
            {
                backNeighbourBlocks.Dispose();
                backNeighbourBlocks =
                    new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        backNeighbourBlocks[x * ChunkData.ChunkSize + y] =
                            UnsafeUtility.ReadArrayElement<BlockData>(AddressByNeighbour[3].ToPointer(),
                                x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1);
                    }
                }
            }


            var rightNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[4] != IntPtr.Zero)
            {
                rightNeighbourBlocks.Dispose();
                rightNeighbourBlocks =
                    new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        rightNeighbourBlocks[y * ChunkData.ChunkSize + z] =
                            UnsafeUtility.ReadArrayElement<BlockData>(AddressByNeighbour[4].ToPointer(),
                                y * ChunkData.ChunkSize + z);
                    }
                }
            }

            var leftNeighbourBlocks = new NativeArray<BlockData>(0, Allocator.Temp);
            if (AddressByNeighbour[5] != IntPtr.Zero)
            {
                leftNeighbourBlocks.Dispose();
                leftNeighbourBlocks =
                    new NativeArray<BlockData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        leftNeighbourBlocks[y * ChunkData.ChunkSize + z] = UnsafeUtility.ReadArrayElement<BlockData>(
                            AddressByNeighbour[5].ToPointer(),
                            (ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared +
                            y * ChunkData.ChunkSize + z);
                    }
                }
            }

            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                Faces[i] = Data.Faces.None;
                if (!Blocks[i].IsSolid()) continue;
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (CheckTopFace(x, y, z, upperNeighbourBlocks, AddressByNeighbour[0]))
                {
                    Faces[i] |= Data.Faces.Top;
                    ChunkGeneratorHelper.GenerateTopSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }

                if (CheckBottomFace(x, y, z, lowerNeighboursBlocks, AddressByNeighbour[1]))
                {
                    Faces[i] |= Data.Faces.Bottom;
                    ChunkGeneratorHelper.GenerateBottomSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }

                if (CheckFrontFace(x, y, z, frontNeighbourBlocks, AddressByNeighbour[2]))
                {
                    Faces[i] |= Data.Faces.Front;
                    ChunkGeneratorHelper.GenerateFrontSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }

                if (CheckBackFace(x, y, z, backNeighbourBlocks, AddressByNeighbour[3]))
                {
                    Faces[i] |= Data.Faces.Back;
                    ChunkGeneratorHelper.GenerateBackSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }

                if (CheckRightFace(x, y, z, rightNeighbourBlocks, AddressByNeighbour[4]))
                {
                    Faces[i] |= Data.Faces.Right;
                    ChunkGeneratorHelper.GenerateRightSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }

                if (CheckLeftFace(x, y, z, leftNeighbourBlocks, AddressByNeighbour[5]))
                {
                    Faces[i] |= Data.Faces.Left;
                    ChunkGeneratorHelper.GenerateLeftSide(x, y, z, Blocks[i].Color, Vertices, Normals, Colors,
                        Triangles);
                }
            }

            upperNeighbourBlocks.Dispose();
            lowerNeighboursBlocks.Dispose();
            frontNeighbourBlocks.Dispose();
            backNeighbourBlocks.Dispose();
            rightNeighbourBlocks.Dispose();
            leftNeighbourBlocks.Dispose();
        }

        private bool CheckTopFace(int x, int y, int z, NativeArray<BlockData> upperNeighbourBlocks,
            IntPtr upperNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y + 1, z) &&
                   (upperNeighbourAddress == IntPtr.Zero ||
                    upperNeighbourAddress != IntPtr.Zero &&
                    !upperNeighbourBlocks[x * ChunkData.ChunkSize + z].IsSolid()) ||
                   ChunkData.IsValidPosition(x, y + 1, z) &&
                   !Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckBottomFace(int x, int y, int z, NativeArray<BlockData> lowerNeighboursBlocks,
            IntPtr lowerNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y - 1, z) && lowerNeighbourAddress != IntPtr.Zero &&
                   !lowerNeighboursBlocks[x * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x, y - 1, z) &&
                   !Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckFrontFace(int x, int y, int z, NativeArray<BlockData> frontNeighbourBlocks,
            IntPtr frontNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y, z + 1) && frontNeighbourAddress != IntPtr.Zero &&
                   !frontNeighbourBlocks[x * ChunkData.ChunkSize + y].IsSolid() ||
                   ChunkData.IsValidPosition(x, y, z + 1) &&
                   !Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].IsSolid();
        }

        private bool CheckBackFace(int x, int y, int z, NativeArray<BlockData> backNeighbourBlocks,
            IntPtr backNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y, z - 1) && backNeighbourAddress != IntPtr.Zero &&
                   !backNeighbourBlocks[x * ChunkData.ChunkSize + y].IsSolid() ||
                   ChunkData.IsValidPosition(x, y, z - 1) &&
                   !Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].IsSolid();
        }


        private bool CheckRightFace(int x, int y, int z, NativeArray<BlockData> rightNeighbourBlocks,
            IntPtr rightNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x + 1, y, z) && rightNeighbourAddress != IntPtr.Zero &&
                   !rightNeighbourBlocks[y * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x + 1, y, z) &&
                   !Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckLeftFace(int x, int y, int z, NativeArray<BlockData> leftNeighbourBlocks,
            IntPtr leftNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x - 1, y, z) && leftNeighbourAddress != IntPtr.Zero &&
                   !leftNeighbourBlocks[y * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x - 1, y, z) &&
                   !Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].IsSolid();
        }
    }
}