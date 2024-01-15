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
        Tnt CreateTnt(Vector3 position, Quaternion rotation, TntItem tntItem, IServer server,
            NetworkConnectionToClient owner, AudioService audioService,
            Vector3Int linkedPosition);

        Grenade CreateGrenade(Vector3 position, Vector3 force, GrenadeItem grenadeItem, IServer server,
            NetworkConnectionToClient owner, AudioService audioService);

        GameObject CreateTombstone(Vector3 position, IServer server, NetworkConnectionToClient owner);

        void CreateRocket(Vector3 position, Quaternion rotation, RocketLauncherItem rocketLauncher,
            IServer server, NetworkConnectionToClient owner, AudioService audioService);

        LootBox CreateAmmoBox(Vector3 position, Transform parent);

        LootBox CreateHealthBox(Vector3 position, Transform parent);

        LootBox CreateBlockBox(Vector3 position, Transform parent);

        SpawnPoint CreateSpawnPoint(Vector3 position, Transform parent);
        
        GameObject CreateDrill(Vector3 position, Quaternion rotation, DrillItem drillData,
            IServer server, NetworkConnectionToClient owner, AudioService audioService, Vector3 direction);
    }
}