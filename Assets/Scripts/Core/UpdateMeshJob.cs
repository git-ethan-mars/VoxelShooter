using GamePlay;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Core
{
    [BurstCompile]
    public struct UpdateMeshJob : IJob
    {
        public NativeArray<Block> Blocks;
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
                Faces[i] = Core.Faces.None;
                if (block.Color.IsEquals(BlockColor.Empty)) continue;
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (!ChunkRenderer.IsValidPosition(x, y + 1, z) ||
                    ChunkRenderer.IsValidPosition(x, y + 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Top;
                    ChunkRenderer.GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y - 1, z) ||
                    ChunkRenderer.IsValidPosition(x, y - 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Bottom;
                    ChunkRenderer.GenerateBottomSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z + 1) ||
                    ChunkRenderer.IsValidPosition(x, y, z + 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Front;
                    ChunkRenderer.GenerateFrontSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z - 1) ||
                    ChunkRenderer.IsValidPosition(x, y, z - 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Back;
                    ChunkRenderer.GenerateBackSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x + 1, y, z) ||
                    ChunkRenderer.IsValidPosition(x + 1, y, z) &&
                    Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Right;
                    ChunkRenderer.GenerateRightSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (!ChunkRenderer.IsValidPosition(x - 1, y, z) ||
                    ChunkRenderer.IsValidPosition(x - 1, y, z) &&
                    Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    Faces[i] |= Core.Faces.Left;
                    ChunkRenderer.GenerateLeftSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }
        }
    }
}