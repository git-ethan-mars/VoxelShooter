using System.Collections.Generic;
using Data;
using Unity.Collections;
using UnityEngine;

namespace Core
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
        public NativeList<Vector3> Vertices { get; private set; }
        public NativeList<int> Triangles { get; private set; }
        public NativeList<Color32> Colors { get; private set; }
        public NativeList<Vector3> Normals { get; private set; }

        public void Construct()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
            ChunkMesh = new Mesh();
            Vertices = new NativeList<Vector3>(Allocator.Persistent);
            Triangles = new NativeList<int>(Allocator.Persistent);
            Colors = new NativeList<Color32>(Allocator.Persistent);
            Normals = new NativeList<Vector3>(Allocator.Persistent);
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

        public static void GenerateTopSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.up);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }

        public static void GenerateBottomSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            AddNormals(normals, Vector3.back);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }

        public static void GenerateFrontSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.forward);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }


        public static void GenerateBackSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            AddNormals(normals, Vector3.back);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }

        public static void GenerateRightSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.right);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }

        public static void GenerateLeftSide(int x, int y, int z, Color32 color, NativeList<Vector3> vertices,
            NativeList<Vector3> normals, NativeList<Color32> colors, NativeList<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            AddNormals(normals, Vector3.left);
            AddTriangles(triangles, vertices.Length);
            AddColor(colors, color);
        }

        public void SpawnBlocks(List<Vector3Int> blockPositions, List<Color32> colors)
        {
            var neighbourChunksToRegenerate = Faces.None;
            for (var i = 0; i < blockPositions.Count; i++)
            {
                ChunkData.Blocks[
                    blockPositions[i].x * ChunkData.ChunkSizeSquared + blockPositions[i].y * ChunkData.ChunkSize +
                    blockPositions[i].z] = new BlockData {Color = colors[i]};
                SetFaces(blockPositions[i].x, blockPositions[i].y, blockPositions[i].z);
                SetNeighboursFaces(blockPositions[i]);
                if (blockPositions[i].z == ChunkData.ChunkSize - 1 && FrontNeighbour is not null)
                {
                    FrontNeighbour.SetFaces(blockPositions[i].x, blockPositions[i].y, 0);
                    neighbourChunksToRegenerate |= Faces.Front;
                }

                if (blockPositions[i].z == 0 && BackNeighbour is not null)
                {
                    BackNeighbour.SetFaces(blockPositions[i].x, blockPositions[i].y, ChunkData.ChunkSize - 1);
                    neighbourChunksToRegenerate |= Faces.Back;
                }

                if (blockPositions[i].y == ChunkData.ChunkSize - 1 && UpperNeighbour is not null)
                {
                    UpperNeighbour.SetFaces(blockPositions[i].x, 0, blockPositions[i].z);
                    neighbourChunksToRegenerate |= Faces.Top;
                }

                if (blockPositions[i].y == 0 && LowerNeighbour is not null)
                {
                    LowerNeighbour.SetFaces(blockPositions[i].x, ChunkData.ChunkSize - 1, blockPositions[i].z);
                    neighbourChunksToRegenerate |= Faces.Bottom;
                }

                if (blockPositions[i].x == ChunkData.ChunkSize - 1 && RightNeighbour is not null)
                {
                    RightNeighbour.SetFaces(0, blockPositions[i].y, blockPositions[i].z);
                    neighbourChunksToRegenerate |= Faces.Right;
                }
                else if (blockPositions[i].x == 0 && LeftNeighbour is not null)
                {
                    LeftNeighbour.SetFaces(ChunkData.ChunkSize - 1, blockPositions[i].y, blockPositions[i].z);
                    neighbourChunksToRegenerate |= Faces.Left;
                }
            }

            RegenerateMesh();
            if (neighbourChunksToRegenerate.HasFlag(Faces.Front))
            {
                FrontNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Back))
            {
                BackNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Top))
            {
                UpperNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Bottom))
            {
                LowerNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Right))
            {
                RightNeighbour.RegenerateMesh();
            }

            if (neighbourChunksToRegenerate.HasFlag(Faces.Left))
            {
                LeftNeighbour.RegenerateMesh();
            }
        }


        public static bool IsValidPosition(int x, int y, int z)
        {
            return x is >= 0 and < ChunkData.ChunkSize && y is >= 0 and < ChunkData.ChunkSize &&
                   z is >= 0 and < ChunkData.ChunkSize;
        }

        private void SetNeighboursFaces(Vector3Int blockPosition)
        {
            if (IsValidPosition(blockPosition.x + 1, blockPosition.y, blockPosition.z))
            {
                SetFaces(blockPosition.x + 1, blockPosition.y, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x - 1, blockPosition.y, blockPosition.z))
            {
                SetFaces(blockPosition.x - 1, blockPosition.y, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y + 1, blockPosition.z))
            {
                SetFaces(blockPosition.x, blockPosition.y + 1, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y - 1, blockPosition.z))
            {
                SetFaces(blockPosition.x, blockPosition.y - 1, blockPosition.z);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z + 1))
            {
                SetFaces(blockPosition.x, blockPosition.y, blockPosition.z + 1);
            }

            if (IsValidPosition(blockPosition.x, blockPosition.y, blockPosition.z - 1))
            {
                SetFaces(blockPosition.x, blockPosition.y, blockPosition.z - 1);
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
                var x = i / ChunkData.ChunkSizeSquared;
                var y = (i - x * ChunkData.ChunkSizeSquared) / ChunkData.ChunkSize;
                var z = i - x * ChunkData.ChunkSizeSquared - y * ChunkData.ChunkSize;
                if (ChunkData.Faces[i] == Faces.None) continue;
                var color = ChunkData.Blocks[i].Color;
                if (ChunkData.Faces[i].HasFlag(Faces.Top))
                {
                    GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Bottom))
                {
                    GenerateBottomSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Front))
                {
                    GenerateFrontSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Back))
                {
                    GenerateBackSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Left))
                {
                    GenerateLeftSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Right))
                {
                    GenerateRightSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }

            ApplyMesh();
        }

        private static void AddNormals(NativeList<Vector3> normals, Vector3 normal)
        {
            for (var i = 0; i < 4; i++)
            {
                normals.Add(normal);
            }
        }

        private static void AddTriangles(NativeList<int> triangles, int vertexCount)
        {
            triangles.Add(vertexCount - 4);
            triangles.Add(vertexCount - 3);
            triangles.Add(vertexCount - 2);
            triangles.Add(vertexCount - 3);
            triangles.Add(vertexCount - 1);
            triangles.Add(vertexCount - 2);
        }

        private static void AddColor(NativeList<Color32> colors, Color32 color)
        {
            for (var i = 0; i < 4; i++)
            {
                colors.Add(color);
            }
        }

        private void SetFaces(int x, int y, int z)
        {
            ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] = Faces.None;
            if (ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                .Equals(BlockColor.Empty)) return;
            if (!IsValidPosition(x, y + 1, z) && UpperNeighbour is not null &&
                UpperNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x, y + 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y + 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Top;
            }

            if (!IsValidPosition(x, y - 1, z) && LowerNeighbour is not null && LowerNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (ChunkData.ChunkSize - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y - 1, z) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + (y - 1) * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Bottom;
            }

            if (!IsValidPosition(x, y, z + 1) && FrontNeighbour is not null &&
                FrontNeighbour.ChunkData.Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z + 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z + 1].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Front;
            }

            if (!IsValidPosition(x, y, z - 1) && BackNeighbour is not null &&
                BackNeighbour.ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + ChunkData.ChunkSize - 1].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x, y, z - 1) && ChunkData
                    .Blocks[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z - 1].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Back;
            }

            if (!IsValidPosition(x + 1, y, z) && RightNeighbour is not null &&
                RightNeighbour.ChunkData.Blocks[y * ChunkData.ChunkSize + z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x + 1, y, z) && ChunkData
                    .Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Right;
            }

            if (!IsValidPosition(x - 1, y, z) && LeftNeighbour is not null && LeftNeighbour.ChunkData
                    .Blocks[(ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x - 1, y, z) && ChunkData
                    .Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Left;
            }
        }

        private void OnDestroy()
        {
            Vertices.Dispose();
            Normals.Dispose();
            Colors.Dispose();
            Triangles.Dispose();
            ChunkData.Blocks.Dispose();
            ChunkData.Faces.Dispose();
        }
    }
}