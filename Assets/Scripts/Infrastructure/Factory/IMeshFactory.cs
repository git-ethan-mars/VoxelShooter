using Infrastructure.Services;
using MapLogic;
using Networking.ClientServices;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IMeshFactory : IService
    {
        void CreateFallingMesh(MeshData meshData, FallingMeshParticlePool fallingMeshFallingMeshParticlePool);
        GameObject CreateChunkMeshRender(Vector3 position, Quaternion rotation, Transform parent);
        void CreateWalls(MapProvider mapProvider, Transform parent);
        void CreateGameModel(GameObject prefab, Transform itemPosition);
        GameObject CreateTransparentGameObject(GameObject prefab, Color32 color);
        void CreateWaterPlane(Vector3 position, Vector3 scale, Color32 waterColor);
    }
}