﻿using Data;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MapSynchronization : NetworkBehaviour
    {
        private IMapProvider MapProvider { get; set; }

        public void Construct(IMapProvider mapProvider)
        {
            MapProvider = mapProvider;
        }
        
        [Command]
        public void SendSpawnPointOnServer(Vector3Int position)
        {
            MapProvider.MapData.SpawnPoints.Add(new SpawnPointData()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            });
        }

        [Command]
        public void DeleteAllSpawnPoints()
        {
            MapProvider.MapData.SpawnPoints.Clear();
        }
        
    }
}