using System.Collections.Generic;
using Data;
using Infrastructure.Factory;
using Networking;
using Networking.ClientServices;
using Rendering;
using UnityEngine;

namespace Generators
{
    public class FallMeshGenerator
    {
        private readonly IMeshFactory _meshFactory;
        private readonly FallingMeshParticlePool _fallingMeshParticlePool;
        private readonly List<Vector3> _vertices = new();
        private readonly List<int> _triangles = new();
        private readonly List<Color32> _colors = new();
        private readonly List<Vector3> _normals = new();

        public FallMeshGenerator(CustomNetworkManager networkManager)
        {
            _fallingMeshParticlePool =
                new FallingMeshParticlePool(networkManager.GameFactory, networkManager.ParticleFactory);
            _meshFactory = networkManager.MeshFactory;
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
                ChunkGeneratorHelper.GenerateBottomSide(x, y, z, blocks[i].BlockData.Color, _vertices, _normals,
                    _colors, _triangles);
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
            _meshFactory.CreateFallingMesh(meshData, _fallingMeshParticlePool);
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