using Data;
using Extensions;
using Generators;
using Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Optimization
{
    [BurstCompile]
    public struct ChunkMeshGeneration : IJob
    {
        public NativeArray<BlockData> UpperNeighbour;
        public NativeArray<BlockData> LowerNeighbour;
        public NativeArray<BlockData> LeftNeighbour;
        public NativeArray<BlockData> RightNeighbour;
        public NativeArray<BlockData> FrontNeighbour;
        public NativeArray<BlockData> BackNeighbour;
        public NativeArray<BlockData> Blocks;
        public NativeArray<Faces> Faces;
        public NativeList<Vector3> Vertices;
        public NativeList<Vector3> Normals;
        public NativeList<int> Triangles;
        public NativeList<Color32> Colors;

        public void Execute()
        {
            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                var block = Blocks[i];
                var color = block.Color;
                Faces[i] = Data.Faces.None;
                if (block.Color.IsEquals(BlockColor.empty)) continue;
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (!ChunkData.IsValidPosition(x, y + 1, z) && (UpperNeighbour.Length == 0 || UpperNeighbour.Length != 0 &&
                    UpperNeighbour[x * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.empty)) ||
                    ChunkData.IsValidPosition(x, y + 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Top;
                    ChunkGeneratorHelper.GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkData.IsValidPosition(x, y - 1, z) && LowerNeighbour.Length != 0 &&
                    LowerNeighbour[x * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.empty) ||
                    ChunkData.IsValidPosition(x, y - 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Bottom;
                    ChunkGeneratorHelper.GenerateBottomSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkData.IsValidPosition(x, y, z + 1) && FrontNeighbour.Length != 0 &&
                    FrontNeighbour[x * ChunkData.ChunkSize + y].Color
                        .IsEquals(BlockColor.empty) ||
                    ChunkData.IsValidPosition(x, y, z + 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Front;
                    ChunkGeneratorHelper.GenerateFrontSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkData.IsValidPosition(x, y, z - 1) && BackNeighbour.Length != 0 &&
                    BackNeighbour[x * ChunkData.ChunkSize + y].Color
                        .IsEquals(BlockColor.empty) ||
                    ChunkData.IsValidPosition(x, y, z - 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Back;
                    ChunkGeneratorHelper.GenerateBackSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkData.IsValidPosition(x + 1, y, z) && RightNeighbour.Length != 0 &&
                    RightNeighbour[y * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.empty) ||
                    ChunkData.IsValidPosition(x + 1, y, z) &&
                    Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Right;
                    ChunkGeneratorHelper.GenerateRightSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkData.IsValidPosition(x - 1, y, z) && LeftNeighbour.Length != 0 &&
                    LeftNeighbour[y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.empty) ||
                    ChunkData.IsValidPosition(x - 1, y, z) &&
                    Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.empty))
                {
                    Faces[i] |= Data.Faces.Left;
                    ChunkGeneratorHelper.GenerateLeftSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }
        }
    }
}