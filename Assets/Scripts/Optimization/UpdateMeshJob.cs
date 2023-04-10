using Data;
using Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Optimization
{
    [BurstCompile]
    public struct UpdateMeshJob : IJob
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
                if (block.Color.IsEquals(BlockColor.Empty)) continue;
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (!ChunkRenderer.IsValidPosition(x, y + 1, z) && UpperNeighbour.Length != 0 &&
                    UpperNeighbour[x * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x, y + 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Top;
                    ChunkRenderer.GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y - 1, z) && LowerNeighbour.Length != 0 &&
                    LowerNeighbour[x * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x, y - 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Bottom;
                    ChunkRenderer.GenerateBottomSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z + 1) && FrontNeighbour.Length != 0 &&
                    FrontNeighbour[x * ChunkData.ChunkSize + y].Color
                        .IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x, y, z + 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Front;
                    ChunkRenderer.GenerateFrontSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z - 1) && BackNeighbour.Length != 0 &&
                    BackNeighbour[x * ChunkData.ChunkSize + y].Color
                        .IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x, y, z - 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Back;
                    ChunkRenderer.GenerateBackSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x + 1, y, z) && RightNeighbour.Length != 0 &&
                    RightNeighbour[y * ChunkData.ChunkSize + z].Color.IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x + 1, y, z) &&
                    Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Right;
                    ChunkRenderer.GenerateRightSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x - 1, y, z) && LeftNeighbour.Length != 0 &&
                    LeftNeighbour[y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty) ||
                    ChunkRenderer.IsValidPosition(x - 1, y, z) &&
                    Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Data.Faces.Left;
                    ChunkRenderer.GenerateLeftSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }
        }
    }
}