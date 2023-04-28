using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using MapLogic;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        
        GameObject CreateLocalNetworkManager(bool isLocalBuild);
        GameObject CreateSteamNetworkManager(bool isLocalBuild);
        GameObject CreateMapRenderer(Map map, Dictionary<Vector3Int, BlockData> buffer);
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);

        void CreateWalls(Map map);
    }
}