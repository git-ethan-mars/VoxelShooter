using System.Collections.Generic;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages;
using Networking.Messages.Responses;
using UnityEngine;

namespace Rendering
{
    public class FallMeshGenerator
    {
        private readonly IMeshFactory _meshFactory;
        private readonly List<Vector3> _vertices;
        private readonly List<int> _triangles;
        private readonly List<Color32> _colors;
        private readonly List<Vector3> _normals;

        public FallMeshGenerator(IMeshFactory meshFactory)
        {
            _meshFactory = meshFactory;
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            _colors = new List<Color32>();
            _normals = new List<Vector3>();
            NetworkClient.RegisterHandler<FallBlockResponse>(OnFallBlockMessage);
        }

        private void OnFallBlockMessage(FallBlockResponse response)
        {
            for (var i = 0; i < response.Positions.Length; i++)
            {
                var x = response.Positions[i].x;
                var y = response.Positions[i].y;
                var z = response.Positions[i].z;
                GenerateTopSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
                GenerateBottomSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
                GenerateLeftSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
                GenerateRightSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
                GenerateFrontSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
                GenerateBackSide(x, y, z, response.BlockData[i].Color, _vertices, _normals, _colors,
                    _triangles);
            }

            var meshData = new MeshData(_vertices.ToArray(), _triangles, _colors, _normals);
            _meshFactory.CreateFallingMesh(meshData);
            ClearMeshData();
        }

        private void ClearMeshData()
        {
            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            _normals.Clear();
        }

        private void GenerateTopSide(int x, int y, int z, Color32 color,
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

        private void GenerateBottomSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
            List<Vector3> normals, List<Color32> colors, List<int> triangles)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x + 1, y, z));
            vertices.Add(new Vector3(x, y, z + 1));
            vertices.Add(new Vector3(x + 1, y, z + 1));
            AddNormals(normals, Vector3.back);
            AddTriangles(triangles, vertices.Count);
            AddColor(colors, color);
        }

        private void GenerateFrontSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
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


        private void GenerateBackSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
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

        private void GenerateRightSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
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

        private void GenerateLeftSide(int x, int y, int z, Color32 color, List<Vector3> vertices,
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
    }
}