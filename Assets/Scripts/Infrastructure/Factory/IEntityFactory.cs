using Data;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreateTnt(Vector3 position, Quaternion rotation);

        GameObject CreateGrenade(Vector3 position, Quaternion rotation);

        GameObject CreateTombstone(Vector3 position);

        GameObject CreateRocket(Vector3 position, Quaternion rotation, MapProvider mapProvider,
            MapUpdater mapUpdater,
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner);

        GameObject CreateSpawnPoint(Vector3 position, Transform parent);
    }
}