using System.Collections.Generic;
using Data;
using MapLogic;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MapSynchronization : NetworkBehaviour
    {
        private Map Map { get; set; }

        public void Construct(Map map)
        {
            Map = map;
        }
        
        [Command]
        public void SendSpawnPointOnServer(Vector3Int position)
        {
            Map.MapData.SpawnPoints.Add(new SpawnPoint()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            });
        }

        [Command]
        public void DeleteAllSpawnPoints()
        {
            Map.MapData.SpawnPoints.Clear();
        }
        
    }
}