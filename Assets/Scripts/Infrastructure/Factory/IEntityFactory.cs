﻿using Data;
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
            IParticleFactory particleFactory, RocketLauncherItem rocketData, NetworkConnectionToClient owner);
        
        LootBox CreateLootBox(Vector3 position);

        GameObject CreateSpawnPoint(Vector3 position, Transform parent);
    }
}