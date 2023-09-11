using System.Collections.Generic;
using Infrastructure.Factory;
using Rendering;
using UnityEngine;

namespace Generators
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
        }

        public void GenerateFallBlocks(Vector3Int[] positions, Color32[] colors)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                var x = positions[i].x;
                var y = positions[i].y;
                var z = positions[i].z;
                ChunkGeneratorHelper.GenerateTopSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBottomSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateLeftSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateRightSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateFrontSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBackSide(x, y, z, colors[i], _vertices, _normals, _colors,
                    _triangles);
            }

            var meshData = new MeshData(_vertices, _triangles, _colors, _normals);
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
    }
}