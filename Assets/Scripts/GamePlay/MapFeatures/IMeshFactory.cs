using Common;
using VoxelMap;

namespace GamePlay.MapFeatures
{
    public interface IMeshFactory : IService
    {
        void CreateFallingMesh(MeshData meshData, FallingMeshParticlePool fallingMeshParticlePool);
    }
}