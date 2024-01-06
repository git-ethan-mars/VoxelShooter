using Data;
using Entities;
using Infrastructure.Services;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        Tnt CreateTnt(Vector3 position, Quaternion rotation, IServer server,
            NetworkConnectionToClient owner, TntItem tntItem,
            AudioService audioService);

        Grenade CreateGrenade(Vector3 position, Vector3 force, IServer server,
            NetworkConnectionToClient owner,
            GrenadeItem grenadeItem,
            AudioService audioService);

        GameObject CreateTombstone(Vector3 position);

        GameObject CreateRocket(Vector3 position, Quaternion rotation, IServer server,
            NetworkConnectionToClient owner,
            RocketLauncherItem rocketData,
            AudioService audioService);

        LootBox CreateAmmoBox(Vector3 position, Transform parent);

        LootBox CreateHealthBox(Vector3 position, Transform parent);

        LootBox CreateBlockBox(Vector3 position, Transform parent);

        SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent);
    }
}