using Data;
using Infrastructure.Services;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        CustomNetworkManager CreateLocalNetworkManager(ServerSettings serverSettings);

        CustomNetworkManager CreateSteamNetworkManager(ServerSettings serverSettings,
            bool isHost);

        Transform CreateGameObjectContainer(string containerName);
        void CreateDirectionalLight(LightData lightData);
        AudioSource CreateAudioSource(Transform container);
    }
}