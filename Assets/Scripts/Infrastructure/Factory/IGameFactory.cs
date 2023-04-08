using Core;
using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        GameObject CreatePlayer(Vector3 position, Quaternion rotation);
        GameObject CreatePlayer();

        GameObject CreateBulletHole(Vector3 position, Quaternion rotation);
        GameObject CreateMuzzleFlash(Transform transform);

        GameObject CreateNetworkManager();
        GameObject CreateMapGenerator();
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);
    }
}