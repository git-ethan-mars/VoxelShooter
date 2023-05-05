using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using Infrastructure.States;
using MapLogic;
using Rendering;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        GameObject CreateLocalNetworkManager(GameStateMachine stateMachine, bool isLocalBuild,
            ServerSettings serverSettings);

        GameObject CreateSteamNetworkManager(GameStateMachine stateMachine, bool isLocalBuild,
            ServerSettings serverSettings);

        GameObject CreateMapRenderer(Map map, Dictionary<Vector3Int, BlockData> buffer);
        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);

        void CreateWalls(Map map);
    }
}