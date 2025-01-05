using System.Collections.Generic;
using Common;
using GamePlay.Factory;
using UnityEngine;
using VoxelMap;

namespace GamePlay.MapFeatures
{
    public class FallMeshGenerator
    {
        private readonly IMeshFactory _meshFactory;
        private readonly FallingMeshParticlePool _fallingMeshParticlePool;
        private readonly List<Vector3> _vertices = new();
        private readonly List<int> _triangles = new();
        private readonly List<Color32> _colors = new();
        private readonly List<Vector3> _normals = new();

        public FallMeshGenerator(IParticleFactory particleFactory, IMeshFactory meshFactory)
        {
            _fallingMeshParticlePool =
                new FallingMeshParticlePool(particleFactory);
            _meshFactory = meshFactory;
        }

        public void GenerateFallVoxels(Voxel[] voxels)
        {
            for (var i = 0; i < voxels.Length; i++)
            {
                var x = voxels[i].Position.x;
                var y = voxels[i].Position.y;
                var z = voxels[i].Position.z;
                ChunkGeneratorHelper.GenerateTopSide(x, y, z, voxels[i].Data.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBottomSide(x, y, z, voxels[i].Data.Color, _vertices, _normals,
                    _colors, _triangles);
                ChunkGeneratorHelper.GenerateLeftSide(x, y, z, voxels[i].Data.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateRightSide(x, y, z, voxels[i].Data.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateFrontSide(x, y, z, voxels[i].Data.Color, _vertices, _normals, _colors,
                    _triangles);
                ChunkGeneratorHelper.GenerateBackSide(x, y, z, voxels[i].Data.Color, _vertices, _normals, _colors,
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