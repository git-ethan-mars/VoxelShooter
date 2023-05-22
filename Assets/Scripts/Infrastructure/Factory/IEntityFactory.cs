﻿using Data;
using Infrastructure.Services;
using Mirror;
using Networking;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IEntityFactory : IService
    {
        GameObject CreateTnt(Vector3 position, Quaternion rotation);
        
        GameObject CreateGrenade(Vector3 position, Quaternion rotation);
        
        GameObject CreateRocket(Vector3 rayOrigin, Quaternion identity, ServerData serverData, RocketLauncherItem rocketData, NetworkConnectionToClient owner);
    }
}