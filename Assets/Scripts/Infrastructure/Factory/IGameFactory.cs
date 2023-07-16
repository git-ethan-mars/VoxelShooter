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
        GameObject CreateLocalNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings);

        GameObject CreateSteamNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings, bool isHost);

        ChunkRenderer CreateChunkRenderer(Vector3Int vector3Int, Quaternion identity, Transform transform);
        void CreateMapRenderer(IMapProvider mapProvider, Dictionary<Vector3Int, BlockData> buffer);

        void CreateWalls(IMapProvider mapProvider);
        void CreateCamera();
    }
}