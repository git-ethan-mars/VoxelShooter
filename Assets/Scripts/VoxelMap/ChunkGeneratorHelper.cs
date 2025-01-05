using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace VoxelMap
{
    public static class ChunkGeneratorHelper
    {
        public static void GenerateTopSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices, NativeList<int>
            triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z), Normal = Vector3.up, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.up, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.up, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.up, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static void GenerateBottomSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices,
            NativeList<int> triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z), Normal = Vector3.down, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z), Normal = Vector3.down, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z + 1), Normal = Vector3.down, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.down, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static void GenerateFrontSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices, NativeList<int>
            triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z + 1), Normal = Vector3.forward, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.forward, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.forward, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.forward, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static void GenerateBackSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices, NativeList<int>
            triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z), Normal = Vector3.back, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z), Normal = Vector3.back, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z), Normal = Vector3.back, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.back, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static void GenerateRightSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices, NativeList<int>
            triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z), Normal = Vector3.right, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z), Normal = Vector3.right, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y, z + 1), Normal = Vector3.right, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x + 1, y + 1, z + 1), Normal = Vector3.right, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static void GenerateLeftSide(int x, int y, int z, Color32 color, NativeList<VertexData> vertices, NativeList<int>
            triangles)
        {
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z), Normal = Vector3.left, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y, z + 1), Normal = Vector3.left, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z), Normal = Vector3.left, Color = color });
            vertices.Add(new VertexData() { Position = new Vector3(x, y + 1, z + 1), Normal = Vector3.left, Color = color });
            AddTriangles(triangles, vertices.Length);
        }

        public static bool TryGetChunkNeighbourNumber(ChunkNeighbourType type, int sourceChunkNumber, int mapHeight, int mapDepth, int chunkCount,
            out int neighbourChunkNumber)
        {
            switch (type)
            {
                case ChunkNeighbourType.Up:
                    return TryGetUpChunkNumber(sourceChunkNumber, mapHeight, mapDepth, chunkCount, out neighbourChunkNumber);
                case ChunkNeighbourType.Down:
                    return TryGetDownChunkNumber(sourceChunkNumber, mapHeight, mapDepth, out neighbourChunkNumber);
                case ChunkNeighbourType.Front:
                    return TryGetFrontChunkNumber(sourceChunkNumber, mapDepth, chunkCount, out neighbourChunkNumber);
                case ChunkNeighbourType.Back:
                    return TryGetBackChunkNumber(sourceChunkNumber, mapDepth, out neighbourChunkNumber);
                case ChunkNeighbourType.Right:
                    return TryGetRightChunkNumber(sourceChunkNumber, mapHeight, mapDepth, chunkCount, out neighbourChunkNumber);
                case ChunkNeighbourType.Left:
                    return TryGetLeftChunkNumber(sourceChunkNumber, mapHeight, mapDepth, out neighbourChunkNumber);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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

        public static void GenerateTopSide(int x, int y, int z, Color32 color,
            List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.up);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        public static void GenerateBottomSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            AddNormals(normals, Vector3.down);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        public static void GenerateFrontSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.forward);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        public static void GenerateBackSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            AddNormals(normals, Vector3.back);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        public static void GenerateRightSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x + 1, y + 1, z));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            vertices.Add(new Vector3(x + 1, y + 1, z + 1));
            AddNormals(normals, Vector3.right);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        public static void GenerateLeftSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x, y + 1, z));
            vertices.Add(new Vector3(x, y + 1, z + 1));
            AddNormals(normals, Vector3.left);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        private static void AddNormals(List<Vector3> normals, Vector3 normal)
        {
            for (var i = 0; i < 4; i++)
            {
                normals.Add(normal);
            }
        }

        private static void AddTriangles(List<int> triangles, int vertexCount)
        {
            triangles.Add(vertexCount - 4);
            triangles.Add(vertexCount - 3);
            triangles.Add(vertexCount - 2);
            triangles.Add(vertexCount - 3);
            triangles.Add(vertexCount - 1);
            triangles.Add(vertexCount - 2);
        }

        private static void AddColor(List<Color32> colors, Color32 color)
        {
            for (var i = 0; i < 4; i++)
            {
                colors.Add(color);
            }
        }

        private static bool TryGetFrontChunkNumber(int chunkNumber, int depth, int chunkCount, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber + 1;
            if (notValidatedChunkNumber < chunkCount &&
                chunkNumber / (depth / ChunkData.ChunkSize) ==
                notValidatedChunkNumber / (depth / ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        private static bool TryGetBackChunkNumber(int chunkNumber, int depth, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber - 1;
            if (notValidatedChunkNumber >= 0 && chunkNumber / (depth / ChunkData.ChunkSize) ==
                notValidatedChunkNumber / (depth / ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        private static bool TryGetUpChunkNumber(int chunkNumber, int height, int depth, int chunkCount, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber + depth / ChunkData.ChunkSize;
            if (notValidatedChunkNumber < chunkCount &&
                chunkNumber / (depth / ChunkData.ChunkSize * height /
                               ChunkData.ChunkSize) ==
                notValidatedChunkNumber /
                (depth / ChunkData.ChunkSize * height /
                 ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        private static bool TryGetDownChunkNumber(int chunkNumber, int height, int depth, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber - depth / ChunkData.ChunkSize;
            if (notValidatedChunkNumber >= 0 &&
                chunkNumber / (depth / ChunkData.ChunkSize * height /
                               ChunkData.ChunkSize) ==
                notValidatedChunkNumber /
                (depth / ChunkData.ChunkSize * height /
                 ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        private static bool TryGetRightChunkNumber(int chunkNumber, int height, int depth, int chunkCount, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber =
                chunkNumber + height / ChunkData.ChunkSize * depth / ChunkData.ChunkSize;

            if (notValidatedChunkNumber < chunkCount)
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        private static bool TryGetLeftChunkNumber(int chunkNumber, int height, int depth, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber =
                chunkNumber - height / ChunkData.ChunkSize * depth /
                ChunkData.ChunkSize;

            if (notValidatedChunkNumber >= 0)
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }
    }
}