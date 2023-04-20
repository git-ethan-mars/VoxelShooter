using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using MapLogic;
using Networking.Synchronization;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        
        GameObject CreateLocalNetworkManager(MapMessageHandler mapSynchronization);
        GameObject CreateSteamNetworkManager(MapMessageHandler mapSynchronization);
        GameObject CreateMapRenderer(Map map, Dictionary<Vector3Int, BlockData> buffer);
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);
        GameObject CreateHud(GameObject player);
        GameObject CreateGameModel(GameObject model, Transform parent);
        GameObject CreateChangeClassMenu();
        GameObject CreateMapSynchronization();
    }
}