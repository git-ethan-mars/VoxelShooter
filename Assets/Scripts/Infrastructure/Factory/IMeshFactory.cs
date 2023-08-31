using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IMeshFactory : IService
    {
        void CreateFallingMesh(Rendering.MeshData meshData);
        GameObject CreateGameModel(GameObject prefab, Transform itemPosition);
    }
}