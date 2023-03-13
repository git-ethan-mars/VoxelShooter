using Core;
using Unity.Collections;
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
        public NativeList<Vector3> Vertices { get; private set; }
        public NativeList<int> Triangles { get; private set; }
        public NativeList<Color32> Colors { get; private set; }
        public NativeList<Vector3> Normals { get; private set; }

        public void InitializeData()
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

        private void RegenerateMesh()
        {
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
                    GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }

                if (ChunkData.Faces[i].HasFlag(Faces.Right))
                {
                    GenerateTopSide(x, y, z, color, Vertices, Normals, Colors, Triangles);
                }
            }

            ApplyMesh();
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            ChunkMesh.Clear();
            Normals.Clear();
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

        private void GenerateBlock(int x, int y, int z)
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

            if (!IsValidPosition(x + 1, y, z) && LeftNeighbour is not null &&
                LeftNeighbour.ChunkData.Blocks[y * ChunkData.ChunkSize + z].Color.Equals(BlockColor.Empty) ||
                IsValidPosition(x + 1, y, z) && ChunkData
                    .Blocks[(x + 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Right;
            }

            if (!IsValidPosition(x - 1, y, z) && RightNeighbour is not null && RightNeighbour.ChunkData
                    .Blocks[(ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty) ||
                IsValidPosition(x - 1, y, z) && ChunkData
                    .Blocks[(x - 1) * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z].Color
                    .Equals(BlockColor.Empty))
            {
                ChunkData.Faces[x * ChunkData.ChunkSizeSquared + y * ChunkData.ChunkSize + z] |= Faces.Left;
            }
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
            if (blockPosition.z == ChunkData.ChunkSize - 1 && FrontNeighbour is not null)
            {
                FrontNeighbour.GenerateBlock(blockPosition.x, blockPosition.y, 0);
                FrontNeighbour.RegenerateMesh();
            }

            if (blockPosition.z == 0 && BackNeighbour is not null)
            {
                BackNeighbour.GenerateBlock(blockPosition.x, blockPosition.y, ChunkData.ChunkSize - 1);
                BackNeighbour.RegenerateMesh();
            }

            if (blockPosition.y == ChunkData.ChunkSize - 1 && UpperNeighbour is not null)
            {
                UpperNeighbour.GenerateBlock(blockPosition.x, 0, blockPosition.z);
                UpperNeighbour.RegenerateMesh();
            }

            if (blockPosition.y == 0 && LowerNeighbour is not null)
            {
                LowerNeighbour.GenerateBlock(blockPosition.x, ChunkData.ChunkSize - 1, blockPosition.z);
                LowerNeighbour.RegenerateMesh();
            }

            if (blockPosition.x == ChunkData.ChunkSize - 1 && RightNeighbour is not null)
            {
                RightNeighbour.GenerateBlock(0, blockPosition.y, blockPosition.z);
                RightNeighbour.RegenerateMesh();
            }
            else if (blockPosition.x == 0 && LeftNeighbour is not null)
            {
                LeftNeighbour.GenerateBlock(ChunkData.ChunkSize - 1, blockPosition.y, blockPosition.z);
                LeftNeighbour.RegenerateMesh();
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
            ChunkData.Faces.Dispose();
        }
    }
}