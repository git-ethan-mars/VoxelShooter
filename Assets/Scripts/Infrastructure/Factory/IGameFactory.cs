using Data;
using Infrastructure.Services;
using Infrastructure.States;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        CustomNetworkManager CreateLocalNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings);

        CustomNetworkManager CreateSteamNetworkManager(GameStateMachine stateMachine, ServerSettings serverSettings,
            bool isHost);

        Transform CreateGameObjectContainer(string containerName);
        void CreateDirectionalLight(LightData lightData);
        AudioSource CreateAudioSource(Transform container);
    }
}