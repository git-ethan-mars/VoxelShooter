using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ChunkRenderer : MonoBehaviour
    {
        private Mesh ChunkMesh { get; set; }
        private MeshCollider MeshCollider { get; set; }
        private MeshFilter MeshFilter { get; set; }
        public ChunkData ChunkData { get; set; }
        private List<Vector3> Vertices { get; set; }
        private List<int> Triangles { get; set; }
        private List<Vector2> UV { get; set; }

        private void Start()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ChunkMesh = new Mesh();
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            UV = new List<Vector2>();
            RegenerateMesh();
        }


        private void RegenerateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();
            UV.Clear();
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
            ChunkMesh.uv = UV.ToArray();
            ChunkMesh.RecalculateNormals();
            ChunkMesh.RecalculateBounds();
            if (Vertices.Count == 0)
            {
                MeshFilter.mesh.Clear();
                if (MeshCollider.sharedMesh is not null)
                    MeshCollider.sharedMesh.Clear();
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
            if (ChunkData.Blocks[x, y, z].Kind == BlockKind.Empty) return;
            var blockPosition = new Vector3Int(x, y, z);
            if (!IsValidPosition(x, y + 1, z) ||
                (IsValidPosition(x, y + 1, z) && ChunkData.Blocks[x, y + 1, z].Kind == BlockKind.Empty))
            {
                GenerateTopSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }

            if (!IsValidPosition(x, y - 1, z) ||
                (IsValidPosition(x, y - 1, z) && ChunkData.Blocks[x, y - 1, z].Kind == BlockKind.Empty))
            {
                GenerateBottomSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }

            if (!IsValidPosition(x, y, z + 1) ||
                (IsValidPosition(x, y, z + 1) && ChunkData.Blocks[x, y, z + 1].Kind == BlockKind.Empty))
            {
                GenerateFrontSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }

            if (!IsValidPosition(x, y, z - 1) ||
                (IsValidPosition(x, y, z - 1) && ChunkData.Blocks[x, y, z - 1].Kind == BlockKind.Empty))
            {
                GenerateBackSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }

            if (!IsValidPosition(x - 1, y, z) ||
                (IsValidPosition(x - 1, y, z) && ChunkData.Blocks[x - 1, y, z].Kind == BlockKind.Empty))
            {
                GenerateRightSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }

            if (!IsValidPosition(x + 1, y, z) ||
                (IsValidPosition(x + 1, y, z) && ChunkData.Blocks[x + 1, y, z].Kind == BlockKind.Empty))
            {
                GenerateLeftSide(blockPosition);
                AddUv(ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z].Kind);
            }
        }

        private void AddUv(BlockKind blockKind)
        {
            if (blockKind == BlockKind.Red)
            {
                UV.Add(new Vector2(128f / 512 + 0.01f, 480f / 512+0.01f));
                UV.Add(new Vector2(128f / 512+0.01f, 1));
                UV.Add(new Vector2(160f / 512-0.01f, 480f / 512+0.01f));
                UV.Add(new Vector2(160f / 512-0.01f, 1));
            }

            if (blockKind == BlockKind.Green)
            {
                UV.Add(new Vector2(64f / 512+0.01f, 480f / 512+0.01f));
                UV.Add(new Vector2(64f / 512+0.01f, 1));
                UV.Add(new Vector2(96f / 512-0.01f, 480f / 512+0.01f));
                UV.Add(new Vector2(96f / 512-0.01f, 1));
            }

            if (blockKind == BlockKind.Blue)
            {
                UV.Add(new Vector2(0f / 512, 32f / 512+0.01f));
                UV.Add(new Vector2(0f / 512, 64f / 512-0.01f));
                UV.Add(new Vector2(32f / 512-0.01f, 32f / 512+0.01f));
                UV.Add(new Vector2(32f / 512-0.01f, 64f / 512-0.01f));
            }
        }

        public void SpawnBlock(Vector3Int blockPosition, Block block)
        {
            Debug.Log($"{blockPosition} {block.Kind}");
            ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = block;
            RegenerateMesh();
        }

        private static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }
    }
}