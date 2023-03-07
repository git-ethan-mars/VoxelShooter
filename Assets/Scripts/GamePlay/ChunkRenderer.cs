using System.Collections.Generic;
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
        private List<Vector3> Vertices { get; set; }
        private List<int> Triangles { get; set; }
        private List<Color32> Colors { get; set; }
        private BlockMesh[] BlockMeshes { get; set; }

        private void Start()
        {
            BlockMeshes = new BlockMesh[ChunkData.ChunkSizeCubed];
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ChunkMesh = new Mesh();
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Colors = new List<Color32>();
            CreateMesh();
        }

        private void CreateMesh()
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

            RegenerateMesh();
        }


        private void RegenerateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            ChunkMesh.Clear();
            for (var i = 0; i < ChunkData.ChunkSizeCubed; i++)
            {
                if (BlockMeshes[i].FacesCount == 0) continue;
                Vertices.AddRange(BlockMeshes[i].Vertices);
                for (var j = 0; j < BlockMeshes[i].FacesCount; j++)
                {
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 4);
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 3);
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 2);
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 3);
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 1);
                    Triangles.Add(Vertices.Count - 4 * (BlockMeshes[i].FacesCount - 1 - j) - 2);
                    for (var k = 0; k < 4; k++)
                    {
                        Colors.Add(BlockMeshes[i].Color);
                    }
                }
            }

            ChunkMesh.vertices = Vertices.ToArray();
            ChunkMesh.triangles = Triangles.ToArray();
            ChunkMesh.colors32 = Colors.ToArray();
            ChunkMesh.RecalculateNormals();
            ChunkMesh.RecalculateBounds();
            if (Vertices.Count == 0)
            {
                MeshCollider.sharedMesh = null;
            }
            else
            {
                MeshFilter.mesh = ChunkMesh;
                MeshCollider.sharedMesh = ChunkMesh;
            }
        }


        private static void GenerateTopSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }

        private static void GenerateBottomSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
        }

        private static void GenerateFrontSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }


        private static void GenerateBackSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
        }

        private static void GenerateRightSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(0, 0, 0));
            vertices.Add(position + new Vector3Int(0, 0, 1));
            vertices.Add(position + new Vector3Int(0, 1, 0));
            vertices.Add(position + new Vector3Int(0, 1, 1));
        }

        private static void GenerateLeftSide(Vector3Int position, List<Vector3> vertices)
        {
            vertices.Add(position + new Vector3Int(1, 0, 0));
            vertices.Add(position + new Vector3Int(1, 1, 0));
            vertices.Add(position + new Vector3Int(1, 0, 1));
            vertices.Add(position + new Vector3Int(1, 1, 1));
        }

        private void GenerateBlock(int x, int y, int z)
        {
            if (ChunkData.Blocks[x, y, z].Color.Equals(BlockColor.Empty)) return;
            var blockPosition = new Vector3Int(x, y, z);
            var color = ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color;
            var facesCount = 0;
            var vertices = new List<Vector3>();
            if (!IsValidPosition(x, y + 1, z) && UpperNeighbour is not null &&
                UpperNeighbour.ChunkData.Blocks[x, 0, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y + 1, z) && ChunkData.Blocks[x, y + 1, z].Color.Equals(BlockColor.Empty))
            {
                GenerateTopSide(blockPosition, vertices);
                facesCount++;
            }

            if (!IsValidPosition(x, y - 1, z) && LowerNeighbour is not null && LowerNeighbour.ChunkData
                    .Blocks[x, ChunkData.ChunkSize - 1, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y - 1, z) && ChunkData.Blocks[x, y - 1, z].Color.Equals(BlockColor.Empty))
            {
                GenerateBottomSide(blockPosition, vertices);
                facesCount++;
            }

            if (!IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                FrontNeighbour.ChunkData.Blocks[x, y, 0].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z + 1) && ChunkData.Blocks[x, y, z + 1].Color.Equals(BlockColor.Empty))
            {
                GenerateFrontSide(blockPosition, vertices);
                facesCount++;
            }

            if (!IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                BackNeighbour.ChunkData.Blocks[x, y, ChunkData.ChunkSize - 1].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z - 1) && ChunkData.Blocks[x, y, z - 1].Color.Equals(BlockColor.Empty))
            {
                GenerateBackSide(blockPosition, vertices);
                facesCount++;
            }

            if (!IsValidPosition(x + 1, y, z) && LeftNeighbour is not null &&
                LeftNeighbour.ChunkData.Blocks[0, y, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x + 1, y, z) && ChunkData.Blocks[x + 1, y, z].Color.Equals(BlockColor.Empty))
            {
                GenerateLeftSide(blockPosition, vertices);
                facesCount++;
            }

            if (!IsValidPosition(x - 1, y, z) && RightNeighbour is not null && RightNeighbour.ChunkData
                    .Blocks[ChunkData.ChunkSize - 1, y, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x - 1, y, z) && ChunkData.Blocks[x - 1, y, z].Color.Equals(BlockColor.Empty))
            {
                GenerateRightSide(blockPosition, vertices);
                facesCount++;
            }

            BlockMeshes[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] =
                new BlockMesh(vertices.ToArray(), color, facesCount);
        }


        public void SpawnBlock(Vector3Int blockPosition, Block block)
        {
            ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = block;
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

        private static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }

        private struct BlockMesh
        {
            public readonly Vector3[] Vertices;
            public readonly int FacesCount;
            public readonly Color32 Color;

            public BlockMesh(Vector3[] vertices, Color32 color, int facesCount)
            {
                Vertices = vertices;
                Color = color;
                FacesCount = facesCount;
            }
        }
    }
}