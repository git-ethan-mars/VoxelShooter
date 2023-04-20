using System.Collections.Generic;
using Data;
using MapLogic;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace PlayerLogic
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


        [Command]
        public void UpdateBlocksOnServer(Vector3Int[] globalPositions, BlockData[] blocks)
        {
            var validPositionList = new List<Vector3Int>();
            var validBlockDataList = new List<BlockData>();
            for (var i = 0; i < globalPositions.Length; i++)
            {
                if (!Map.IsValidPosition(globalPositions[i])) continue;
                var currentBlock = Map.GetBlockByGlobalPosition(globalPositions[i]);
                if (currentBlock.Equals(blocks[i])) continue;
                validPositionList.Add(globalPositions[i]);
                validBlockDataList.Add(blocks[i]);
            }
            var updateMapMessage = new UpdateMapMessage(validPositionList.ToArray(), validBlockDataList.ToArray());
            NetworkServer.SendToAll(updateMapMessage);
        }
    }
}