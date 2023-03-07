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

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ChunkMesh = new Mesh();
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Colors = new List<Color32>();
            RegenerateMesh();
        }


        private void RegenerateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            ChunkMesh.Clear();
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


        private void GenerateTopSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(0, 1, 0));
            Vertices.Add(position + new Vector3Int(0, 1, 1));
            Vertices.Add(position + new Vector3Int(1, 1, 0));
            Vertices.Add(position + new Vector3Int(1, 1, 1));
            AddTriangles();
        }

        private void GenerateBottomSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(0, 0, 0));
            Vertices.Add(position + new Vector3Int(1, 0, 0));
            Vertices.Add(position + new Vector3Int(0, 0, 1));
            Vertices.Add(position + new Vector3Int(1, 0, 1));
            AddTriangles();
        }

        private void GenerateFrontSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(0, 0, 1));
            Vertices.Add(position + new Vector3Int(1, 0, 1));
            Vertices.Add(position + new Vector3Int(0, 1, 1));
            Vertices.Add(position + new Vector3Int(1, 1, 1));
            AddTriangles();
        }


        private void GenerateBackSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(0, 0, 0));
            Vertices.Add(position + new Vector3Int(0, 1, 0));
            Vertices.Add(position + new Vector3Int(1, 0, 0));
            Vertices.Add(position + new Vector3Int(1, 1, 0));
            AddTriangles();
        }

        private void GenerateRightSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(0, 0, 0));
            Vertices.Add(position + new Vector3Int(0, 0, 1));
            Vertices.Add(position + new Vector3Int(0, 1, 0));
            Vertices.Add(position + new Vector3Int(0, 1, 1));
            AddTriangles();
        }

        private void GenerateLeftSide(Vector3Int position)
        {
            Vertices.Add(position + new Vector3Int(1, 0, 0));
            Vertices.Add(position + new Vector3Int(1, 1, 0));
            Vertices.Add(position + new Vector3Int(1, 0, 1));
            Vertices.Add(position + new Vector3Int(1, 1, 1));
            AddTriangles();
        }

        private void AddTriangles()
        {
            Triangles.Add(Vertices.Count - 4);
            Triangles.Add(Vertices.Count - 3);
            Triangles.Add(Vertices.Count - 2);
            Triangles.Add(Vertices.Count - 3);
            Triangles.Add(Vertices.Count - 1);
            Triangles.Add(Vertices.Count - 2);
        }

        private void GenerateBlock(int x, int y, int z)
        {
            if (ChunkData.Blocks[x, y, z].Color.Equals(BlockColor.Empty)) return;
            var blockPosition = new Vector3Int(x, y, z);
            if (!IsValidPosition(x, y + 1, z) && UpperNeighbour is not null &&
                UpperNeighbour.ChunkData.Blocks[x, 0, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y + 1, z) && ChunkData.Blocks[x, y + 1, z].Color.Equals(BlockColor.Empty))
            {
                GenerateTopSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }

            if (!IsValidPosition(x, y - 1, z) && LowerNeighbour is not null && LowerNeighbour.ChunkData
                    .Blocks[x, ChunkData.ChunkSize - 1, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y - 1, z) && ChunkData.Blocks[x, y - 1, z].Color.Equals(BlockColor.Empty))
            {
                GenerateBottomSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }

            if (!IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                FrontNeighbour.ChunkData.Blocks[x, y, 0].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z + 1) && ChunkData.Blocks[x, y, z + 1].Color.Equals(BlockColor.Empty))
            {
                GenerateFrontSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }

            if (!IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                BackNeighbour.ChunkData.Blocks[x, y, ChunkData.ChunkSize - 1].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z - 1) && ChunkData.Blocks[x, y, z - 1].Color.Equals(BlockColor.Empty))
            {
                GenerateBackSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }

            if (!IsValidPosition(x + 1, y, z) && LeftNeighbour is not null &&
                LeftNeighbour.ChunkData.Blocks[0, y, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x + 1, y, z) && ChunkData.Blocks[x + 1, y, z].Color.Equals(BlockColor.Empty))
            {
                GenerateLeftSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }

            if (!IsValidPosition(x - 1, y, z) && RightNeighbour is not null && RightNeighbour.ChunkData
                    .Blocks[ChunkData.ChunkSize - 1, y, z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x - 1, y, z) && ChunkData.Blocks[x - 1, y, z].Color.Equals(BlockColor.Empty))
            {
                GenerateRightSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Color);
            }
        }

        private void AddUv(Color32 color)
        {
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
            Colors.Add(color);
        }

        public void SpawnBlock(Vector3Int blockPosition, Block block)
        {
            ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = block;
            RegenerateMesh();
            if (blockPosition.z == ChunkData.ChunkSize - 1 && FrontNeighbour is not null)
            {
                FrontNeighbour.RegenerateMesh();
            }

            if (blockPosition.z == 0 && BackNeighbour is not null)
            {
                BackNeighbour.RegenerateMesh();
            }

            if (blockPosition.y == ChunkData.ChunkSize - 1 && UpperNeighbour is not null)
            {
                UpperNeighbour.RegenerateMesh();
            }

            if (blockPosition.y == 0 && LowerNeighbour is not null)
            {
                LowerNeighbour.RegenerateMesh();
            }

            if (blockPosition.x == ChunkData.ChunkSize - 1 && LeftNeighbour is not null)
            {
                LeftNeighbour.RegenerateMesh();
            }

            if (blockPosition.x == 0 && RightNeighbour is not null)
            {
                RightNeighbour.RegenerateMesh();
            }
        }

        private static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }
    }
}