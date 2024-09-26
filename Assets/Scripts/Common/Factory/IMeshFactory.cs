using UnityEngine;

namespace Common.Factory
{
    public interface IMeshFactory : IService
    {
        void CreateFallingMesh(MeshData meshData, FallingMeshParticlePool fallingMeshParticlePool);
        void CreateGameModel(GameObject prefab, Transform itemPosition);
        GameObject CreateTransparentTnt();
        GameObject CreatTransparentBlock();
    }
}