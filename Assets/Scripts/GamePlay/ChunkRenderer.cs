using System.Collections.Generic;
using Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        public ChunkRenderer UpperNeighbour { get; set; }
        public ChunkRenderer LowerNeighbour { get; set; }
        public ChunkRenderer LeftNeighbour { get; set; }
        public ChunkRenderer RightNeighbour { get; set; }
        public ChunkRenderer FrontNeighbour { get; set; }
        public ChunkRenderer BackNeighbour { get; set; }
        private Mesh ChunkMesh { get; set; }
        private MeshCollider MeshCollider { get; set; }
        private MeshFilter MeshFilter { get; set; }
        public ChunkData ChunkData { get; set; }
        public NativeList<Vector3> Vertices { get; set; }
        public NativeList<int> Triangles { get; set; }
        public NativeList<Color32> Colors { get; set; }
        public NativeList<Vector3> Normals { get; set; }
        private BlockMesh[] BlockMeshes { get; set; }

        public void InitializeData()
        {
            BlockMeshes = new BlockMesh[ChunkData.ChunkSizeCubed];
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ChunkMesh = new Mesh();
            Vertices = new NativeList<Vector3>(Allocator.Persistent);
            Triangles = new NativeList<int>(Allocator.Persistent);
            Colors = new NativeList<Color32>(Allocator.Persistent);
            Normals = new NativeList<Vector3>(Allocator.Persistent);
        }

        public void GenerateBlockMesh()
        {
            for (var x = 0; x < ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < ChunkData.ChunkSize; z++)
                    {
                        GenerateBlock(x, y, z);
                    }
                }
            }
        }


        private void RegenerateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            ChunkMesh.Clear();
            Normals.Clear();
            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                if (BlockMeshes[i].FacesCount == 0) continue;
                for (var j = 0; j < BlockMeshes[i].Vertices.Length; j++)
                    Vertices.Add(BlockMeshes[i].Vertices[j]);
                for (var j = 0; j < BlockMeshes[i].FacesCount; j++)
                {
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 4);
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 3);
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 2);
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 3);
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 1);
                    Triangles.Add(Vertices.Length - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 2);
                    for (var k = 0; k < 4; k++)
                    {
                        Colors.Add(BlockMeshes[i].Color);
                        Normals.Add(BlockMeshes[i].Normals[j]);
                    }
                }
            }

            ApplyMesh();
        }


        public void ApplyMesh()
        {
            ChunkMesh.SetVertices(Vertices.AsArray());
            var triangles = new int[Triangles.Length];
            for (var i = 0; i < Triangles.Length; i++)
            {
                triangles[i] = Triangles[i];
            }

            ChunkMesh.SetTriangles(triangles, 0);
            ChunkMesh.SetColors(Colors.AsArray());
            ChunkMesh.SetNormals(Normals.AsArray());
            if (Vertices.Length == 0)
            {
                MeshCollider.sharedMesh = null;
            }
            else
            {
                MeshFilter.mesh = ChunkMesh;
                MeshCollider.sharedMesh = ChunkMesh;
            }
        }


        private static void GenerateTopSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 1, 1));
            normals.Add(Vector3.up);
        }

        private static void GenerateBottomSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            normals.Add(Vector3.back);
        }

        private static void GenerateFrontSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
            normals.Add(Vector3.forward);
        }


        private static void GenerateBackSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            normals.Add(Vector3.back);
        }

        private static void GenerateRightSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            normals.Add(Vector3.left);
        }

        private static void GenerateLeftSide(Vector3Int position, List<Vector3> vertices, List<Vector3> normals)
        {
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
            normals.Add(Vector3.right);
        }

        private void GenerateBlock(int x, int y, int z)
        {
            if (ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                .Equals(BlockColor.Empty))
            {
                BlockMeshes[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] =
                    new BlockMesh(null, BlockColor.Empty, 0, null);
                return;
            }

            var blockPosition = new Vector3Int(x, y, z);
            var color = ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color;
            var facesCount = 0;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            if (!IsValidPosition(x, y + 1, z) && UpperNeighbour is not null &&
                UpperNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y + 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateTopSide(blockPosition, vertices, normals);
                facesCount++;
            }

            if (!IsValidPosition(x, y - 1, z) && LowerNeighbour is not null && LowerNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y - 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateBottomSide(blockPosition, vertices, normals);
                facesCount++;
            }

            if (!IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                FrontNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z + 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateFrontSide(blockPosition, vertices, normals);
                facesCount++;
            }

            if (!IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                BackNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z - 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateBackSide(blockPosition, vertices, normals);
                facesCount++;
            }

            if (!IsValidPosition(x + 1, y, z) && LeftNeighbour is not null &&
                LeftNeighbour.ChunkData.Blocks[y * ChunkData.ChunkSize + z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x + 1, y, z) && ChunkData
                    .Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateLeftSide(blockPosition, vertices, normals);
                facesCount++;
            }

            if (!IsValidPosition(x - 1, y, z) && RightNeighbour is not null && RightNeighbour.ChunkData
                    .Blocks[(ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x - 1, y, z) && ChunkData
                    .Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                GenerateRightSide(blockPosition, vertices, normals);
                facesCount++;
            }

            BlockMeshes[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] =
                new BlockMesh(vertices.ToArray(), color, facesCount, normals.ToArray());
        }


        public void SpawnBlock(Vector3Int blockPosition, Block block)
        {
            ChunkData.Blocks[
                blockPosition.x * ChunkData.ChunkSizeSquared + blockPosition.y * ChunkData.ChunkSize +
                blockPosition.z] = block;
            GenerateBlock(blockPosition.x, blockPosition.y, blockPosition.z);
            if (IsValidPosition(blockPosition.x + 1, blockPosition.y, blockPosition.z))
            {
                GenerateBlock(blockPosition.x + 1, blockPosition.y, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x - 1, blockPosition.y, blockPosition.z))
            {
                GenerateBlock(blockPosition.x - 1, blockPosition.y, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y + 1, blockPosition.z))
            {
                GenerateBlock(blockPosition.x, blockPosition.y + 1, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y - 1, blockPosition.z))
            {
                GenerateBlock(blockPosition.x, blockPosition.y - 1, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z + 1))
            {
                GenerateBlock(blockPosition.x, blockPosition.y, blockPosition.z + 1);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z - 1))
            {
                GenerateBlock(blockPosition.x, blockPosition.y, blockPosition.z - 1);
            }

            RegenerateMesh();
            switch (blockPosition.z)
            {
                case ChunkData.ChunkSize - 1 when FrontNeighbour is not null:
                    FrontNeighbour.GenerateBlock(blockPosition.x, blockPosition.y, 0);
                    FrontNeighbour.RegenerateMesh();
                    break;
                case 0 when BackNeighbour is not null:
                    BackNeighbour.GenerateBlock(blockPosition.x, blockPosition.y, ChunkData.ChunkSize - 1);
                    BackNeighbour.RegenerateMesh();
                    break;
            }

            switch (blockPosition.y)
            {
                case ChunkData.ChunkSize - 1 when UpperNeighbour is not null:
                    UpperNeighbour.GenerateBlock(blockPosition.x, 0, blockPosition.z);
                    UpperNeighbour.RegenerateMesh();
                    break;
                case 0 when LowerNeighbour is not null:
                    LowerNeighbour.GenerateBlock(blockPosition.x, ChunkData.ChunkSize - 1, blockPosition.z);
                    LowerNeighbour.RegenerateMesh();
                    break;
            }

            switch (blockPosition.x)
            {
                case ChunkData.ChunkSize - 1 when LeftNeighbour is not null:
                    LeftNeighbour.GenerateBlock(0, blockPosition.y, blockPosition.z);
                    LeftNeighbour.RegenerateMesh();
                    break;
                case 0 when RightNeighbour is not null:
                    RightNeighbour.GenerateBlock(ChunkData.ChunkSize - 1, blockPosition.y, blockPosition.z);
                    RightNeighbour.RegenerateMesh();
                    break;
            }
        }

        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }

        private void OnDestroy()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Colors.Dispose();
            Triangles.Dispose();
            ChunkData.Blocks.Dispose();
        }

        private struct BlockMesh
        {
            public readonly Vector3[] Vertices;
            public readonly int FacesCount;
            public readonly Color32 Color;
            public readonly Vector3[] Normals;

            public BlockMesh(Vector3[] vertices, Color32 color, int facesCount, Vector3[] normals)
            {
                Vertices = vertices;
                Color = color;
                FacesCount = facesCount;
                Normals = normals;
            }
        }
    }

   
    [BurstCompile]
    public struct UpdateMeshJob : IJob
    {
        public NativeArray<Block> Blocks;
        public NativeList<Vector3> Vertices;
        public NativeList<Vector3> Normals;
        public NativeList<int> Triangles;
        public NativeList<Color32> Color;

        public void Execute()
        {
            for (var index = 0; index < ChunkData.ChunkSizeCubed; index++)
            {
                var block = Blocks[index];
                if (block.Color.IsEquals(BlockColor.Empty)) continue;
                var x = index / ChunkData.ChunkSizeSquared;
                var y = (index - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = index - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                var blockPosition = new Vector3Int(x, y, z);
                if (!ChunkRenderer.IsValidPosition(x, y + 1, z) ||
                    ChunkRenderer.IsValidPosition(x, y + 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateTopSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.up);
                    }
                }

                if (!ChunkRenderer.IsValidPosition(x, y - 1, z) ||
                    ChunkRenderer.IsValidPosition(x, y - 1, z) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateBottomSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.down);
                    }
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z + 1) ||
                    ChunkRenderer.IsValidPosition(x, y, z + 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateFrontSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.forward);
                    }
                }

                if (!ChunkRenderer.IsValidPosition(x, y, z - 1) ||
                    ChunkRenderer.IsValidPosition(x, y, z - 1) &&
                    Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateBackSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.back);
                    }
                }

                if (!ChunkRenderer.IsValidPosition(x + 1, y, z) ||
                    ChunkRenderer.IsValidPosition(x + 1, y, z) &&
                    Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateLeftSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.right);
                    }
                }

                if (!ChunkRenderer.IsValidPosition(x - 1, y, z) ||
                    ChunkRenderer.IsValidPosition(x - 1, y, z) &&
                    Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                        .IsEquals(BlockColor.Empty))
                {
                    GenerateRightSide(blockPosition, Vertices);
                    AddTriangles();
                    AddColor(block);
                    for (var i = 0; i < 4; i++)
                    {
                        Normals.Add(Vector3.left);
                    }
                }
            }
        }

        private void AddTriangles()
        {
            Triangles.Add(Vertices.Length - 4);
            Triangles.Add(Vertices.Length - 3);
            Triangles.Add(Vertices.Length - 2);
            Triangles.Add(Vertices.Length - 3);
            Triangles.Add(Vertices.Length - 1);
            Triangles.Add(Vertices.Length - 2);
        }

        private void AddColor(Block block)
        {
            for (var i = 0; i < 4; i++)
            { 
                Color.Add(block.Color);
            }
        }

        private static void GenerateTopSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }

        private static void GenerateBottomSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
        }

        private static void GenerateFrontSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }


        private static void GenerateBackSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
        }

        private static void GenerateRightSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
        }

        private static void GenerateLeftSide(Vector3Int position, NativeList<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }
    }
}