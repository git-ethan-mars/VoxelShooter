using Data;
using Infrastructure.Services;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        GameObject CreatePlayer(GameClass gameClass, Vector3 position, Quaternion rotation);
        GameObject CreatePlayer(GameClass gameClass);

        GameObject CreateBulletHole(Vector3 position, Quaternion rotation);
        GameObject CreateMuzzleFlash(Transform transform);

        GameObject CreateNetworkManager();
        GameObject CreateMapGenerator();
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);
        GameObject CreateHud(GameObject player);
    }
}