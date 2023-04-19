using Infrastructure.Services;
using MapLogic;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        
        GameObject CreateLocalNetworkManager();
        GameObject CreateSteamNetworkManager();
        GameObject CreateMapGenerator(Map map);
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);
        GameObject CreateHud(GameObject player);
        GameObject CreateGameModel(GameObject model, Transform parent);
        GameObject CreateChangeClassMenu();
    }
}