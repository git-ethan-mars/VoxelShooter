using Unity.Collections;
using UnityEngine;

namespace Generators
{
    public static class ChunkGeneratorHelper
    {
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
    }
}