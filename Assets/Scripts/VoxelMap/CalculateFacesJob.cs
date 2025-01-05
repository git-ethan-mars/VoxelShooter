using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace VoxelMap
{
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CalculateFacesJob : IJob
    {
        [NativeDisableUnsafePtrRestriction]
        public IntPtr UpNeighbourPointer;

        [NativeDisableUnsafePtrRestriction]
        public IntPtr DownNeighbourPointer;

        [NativeDisableUnsafePtrRestriction]
        public IntPtr FrontNeighbourPointer;

        [NativeDisableUnsafePtrRestriction]
        public IntPtr BackNeighbourPointer;

        [NativeDisableUnsafePtrRestriction]
        public IntPtr RightNeighbourPointer;

        [NativeDisableUnsafePtrRestriction]
        public IntPtr LeftNeighbourPointer;

        [ReadOnly]
        public NativeArray<VoxelData> Voxels;
        public NativeArray<Face> Faces;

        public void Execute()
        {
            var upperNeighbourVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (UpNeighbourPointer != IntPtr.Zero)
            {
                upperNeighbourVoxels.Dispose();
                upperNeighbourVoxels = new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory);

                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        upperNeighbourVoxels[x * ChunkData.ChunkSize + z] =
                            UnsafeUtility.ReadArrayElement<VoxelData>(UpNeighbourPointer.ToPointer(),
                                x * ChunkData.ChunkSizeSquared + z);
                    }
                }
            }

            var lowerNeighboursVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (DownNeighbourPointer != IntPtr.Zero)
            {
                lowerNeighboursVoxels.Dispose();
                lowerNeighboursVoxels =
                    new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        lowerNeighboursVoxels[x * ChunkData.ChunkSize + z] = UnsafeUtility.ReadArrayElement<VoxelData>(
                            DownNeighbourPointer.ToPointer(), x * ChunkData.ChunkSizeSquared +
                                                              (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z);
                    }
                }
            }

            var frontNeighbourVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (FrontNeighbourPointer != IntPtr.Zero)
            {
                frontNeighbourVoxels.Dispose();
                frontNeighbourVoxels =
                    new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        frontNeighbourVoxels[x * ChunkData.ChunkSize + y] =
                            UnsafeUtility.ReadArrayElement<VoxelData>(FrontNeighbourPointer.ToPointer(),
                                x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize);
                    }
                }
            }

            var backNeighbourVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (BackNeighbourPointer != IntPtr.Zero)
            {
                backNeighbourVoxels.Dispose();
                backNeighbourVoxels =
                    new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                for (var x = 0; x < ChunkData.ChunkSize; x++)
                {
                    for (var y = 0; y < ChunkData.ChunkSize; y++)
                    {
                        backNeighbourVoxels[x * ChunkData.ChunkSize + y] =
                            UnsafeUtility.ReadArrayElement<VoxelData>(BackNeighbourPointer.ToPointer(),
                                x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1);
                    }
                }
            }


            var rightNeighbourVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (RightNeighbourPointer != IntPtr.Zero)
            {
                rightNeighbourVoxels.Dispose();
                rightNeighbourVoxels =
                    new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        rightNeighbourVoxels[y * ChunkData.ChunkSize + z] =
                            UnsafeUtility.ReadArrayElement<VoxelData>(RightNeighbourPointer.ToPointer(),
                                y * ChunkData.ChunkSize + z);
                    }
                }
            }

            var leftNeighbourVoxels = new NativeArray<VoxelData>(0, Allocator.Temp);
            if (LeftNeighbourPointer != IntPtr.Zero)
            {
                leftNeighbourVoxels.Dispose();
                leftNeighbourVoxels =
                    new NativeArray<VoxelData>(ChunkData.ChunkSizeSquared, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        leftNeighbourVoxels[y * ChunkData.ChunkSize + z] = UnsafeUtility.ReadArrayElement<VoxelData>(
                            LeftNeighbourPointer.ToPointer(),
                            (ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared +
                            y * ChunkData.ChunkSize + z);
                    }
                }
            }

            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                Faces[i] = Face.None;
                if (!Voxels[i].IsSolid())
                {
                    continue;
                }

                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (CheckTopFace(x, y, z, upperNeighbourVoxels, UpNeighbourPointer))
                {
                    Faces[i] |= Face.Top;
                }

                if (CheckBottomFace(x, y, z, lowerNeighboursVoxels, DownNeighbourPointer))
                {
                    Faces[i] |= Face.Bottom;
                }

                if (CheckFrontFace(x, y, z, frontNeighbourVoxels, FrontNeighbourPointer))
                {
                    Faces[i] |= Face.Front;
                }

                if (CheckBackFace(x, y, z, backNeighbourVoxels, BackNeighbourPointer))
                {
                    Faces[i] |= Face.Back;
                }

                if (CheckRightFace(x, y, z, rightNeighbourVoxels, RightNeighbourPointer))
                {
                    Faces[i] |= Face.Right;
                }

                if (CheckLeftFace(x, y, z, leftNeighbourVoxels, LeftNeighbourPointer))
                {
                    Faces[i] |= Face.Left;
                }
            }

            upperNeighbourVoxels.Dispose();
            lowerNeighboursVoxels.Dispose();
            frontNeighbourVoxels.Dispose();
            backNeighbourVoxels.Dispose();
            rightNeighbourVoxels.Dispose();
            leftNeighbourVoxels.Dispose();
        }

        private bool CheckTopFace(int x, int y, int z, NativeArray<VoxelData> upperNeighbourVoxels,
            IntPtr upperNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y + 1, z) &&
                   (upperNeighbourAddress == IntPtr.Zero ||
                    upperNeighbourAddress != IntPtr.Zero &&
                    !upperNeighbourVoxels[x * ChunkData.ChunkSize + z].IsSolid()) ||
                   ChunkData.IsValidPosition(x, y + 1, z) &&
                   !Voxels[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckBottomFace(int x, int y, int z, NativeArray<VoxelData> lowerNeighboursVoxels,
            IntPtr lowerNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y - 1, z) && lowerNeighbourAddress != IntPtr.Zero &&
                   !lowerNeighboursVoxels[x * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x, y - 1, z) &&
                   !Voxels[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckFrontFace(int x, int y, int z, NativeArray<VoxelData> frontNeighbourVoxels,
            IntPtr frontNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y, z + 1) && frontNeighbourAddress != IntPtr.Zero &&
                   !frontNeighbourVoxels[x * ChunkData.ChunkSize + y].IsSolid() ||
                   ChunkData.IsValidPosition(x, y, z + 1) &&
                   !Voxels[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].IsSolid();
        }

        private bool CheckBackFace(int x, int y, int z, NativeArray<VoxelData> backNeighbourVoxels,
            IntPtr backNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x, y, z - 1) && backNeighbourAddress != IntPtr.Zero &&
                   !backNeighbourVoxels[x * ChunkData.ChunkSize + y].IsSolid() ||
                   ChunkData.IsValidPosition(x, y, z - 1) &&
                   !Voxels[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].IsSolid();
        }


        private bool CheckRightFace(int x, int y, int z, NativeArray<VoxelData> rightNeighbourVoxels,
            IntPtr rightNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x + 1, y, z) && rightNeighbourAddress != IntPtr.Zero &&
                   !rightNeighbourVoxels[y * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x + 1, y, z) &&
                   !Voxels[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].IsSolid();
        }

        private bool CheckLeftFace(int x, int y, int z, NativeArray<VoxelData> leftNeighbourVoxels,
            IntPtr leftNeighbourAddress)
        {
            return !ChunkData.IsValidPosition(x - 1, y, z) && leftNeighbourAddress != IntPtr.Zero &&
                   !leftNeighbourVoxels[y * ChunkData.ChunkSize + z].IsSolid() ||
                   ChunkData.IsValidPosition(x - 1, y, z) &&
                   !Voxels[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].IsSolid();
        }
    }
}