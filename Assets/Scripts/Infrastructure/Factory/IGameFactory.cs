using Data;
using Infrastructure.Services;
using Infrastructure.States;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        GameObject CreateLocalNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings);

        GameObject CreateSteamNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings, bool isHost);
        GameObject CreateGameObjectContainer(string containerName);
        void CreateCamera();
        void CreateDirectionalLight(LightData lightData);
    }
}