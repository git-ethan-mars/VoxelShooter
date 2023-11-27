using Data;
using Entities;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreateTnt(Vector3 position, Quaternion rotation);

        GameObject CreateGrenade(Vector3 position, Quaternion rotation);

        GameObject CreateTombstone(Vector3 position);

        GameObject CreateRocket(Vector3 position, Quaternion rotation, IServer server,
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner,
            AudioService audioService);
        
        LootBox CreateAmmoBox(Vector3 position, Transform parent);
        
        LootBox CreateHealthBox(Vector3 position, Transform parent);

        LootBox CreateBlockBox(Vector3 position, Transform parent);

        SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent);
    }
}