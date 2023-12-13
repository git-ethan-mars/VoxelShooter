using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using Networking.ClientServices;
using Rendering;
using UnityEngine;

namespace Generators
{
    public class FallMeshGenerator
    {
        private readonly IMeshFactory _meshFactory;
        private readonly FallingMeshParticlePool _fallingMeshFallingMeshParticlePool;
        private readonly List<Vector3> _vertices;
        private readonly List<int> _triangles;
        private readonly List<Color32> _colors;
        private readonly List<Vector3> _normals;

        public FallMeshGenerator(IMeshFactory meshFactory,
            FallingMeshParticlePool fallingMeshFallingMeshParticlePool)
        {
            _meshFactory = meshFactory;
            _fallingMeshFallingMeshParticlePool = fallingMeshFallingMeshParticlePool;
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            _colors = new List<Color32>();
            _normals = new List<Vector3>();
        }

        public void GenerateFallBlocks(BlockDataWithPosition[] blocks)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                var x = blocks[i].Position.x;
                var y = blocks[i].Position.y;
                var z = blocks[i].Position.z;
                ChunkGeneratorHelper.GenerateTopSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBottomSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateLeftSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateRightSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateFrontSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBackSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals, _colors,
                    _triangles);
            }

            var meshData = new MeshData(_vertices, _triangles, _colors, _normals);
            _meshFactory.CreateFallingMesh(meshData, _fallingMeshFallingMeshParticlePool);
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