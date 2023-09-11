using Infrastructure.Services;
using MapLogic;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IMeshFactory : IService
    {
        void CreateFallingMesh(Rendering.MeshData meshData);
        GameObject CreateChunkMeshRender(Vector3 position, Quaternion rotation, Transform parent);
        void CreateWalls(MapProvider mapProvider, Transform parent);
        GameObject CreateGameModel(GameObject prefab, Transform itemPosition);
    }
}