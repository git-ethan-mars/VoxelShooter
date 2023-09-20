using Data;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MapSynchronization : NetworkBehaviour
    {
        private MapProvider MapProvider { get; set; }

        public void Construct(MapProvider mapProvider)
        {
            MapProvider = mapProvider;
        }

        [Command]
        public void SendSpawnPointOnServer(Vector3Int position)
        {
            MapProvider.SceneData.SpawnPoints.Add(new SpawnPointData
            {
                position = position
            });
        }

        [Command]
        public void DeleteAllSpawnPoints()
        {
            MapProvider.SceneData.SpawnPoints.Clear();
        }
    }
}