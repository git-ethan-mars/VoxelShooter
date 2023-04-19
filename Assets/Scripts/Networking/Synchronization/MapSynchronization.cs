using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using UnityEngine;

namespace Networking.Synchronization
{
    public class MapSynchronization : NetworkBehaviour
    {
        private Map _map;
        private MapGenerator _mapGenerator;

        public void Awake()
        {
            _mapGenerator = AllServices.Container.Single<IMapGeneratorProvider>().MapGenerator;
            _map = _mapGenerator.Map;
        }


        public void CreateExplosion()
        {
        }

        public void CreateSpawnPoint(Vector3Int position)
        {
            SendSpawnPointOnServer(position);
        }

        [Command]
        private void SendSpawnPointOnServer(Vector3Int position)
        {
            _map.MapData.SpawnPoints.Add(new SpawnPoint()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            });
        }
        



        [Command]
        public void UpdateBlocksOnServer(Vector3Int[] globalPositions, BlockData[] blocks)
        {
            var localPositionsByChunkIndex = new Dictionary<int, List<Vector3Int>>();
            var colorsByChunkIndex = new Dictionary<int, List<Color32>>();
            for (var i = 0; i < globalPositions.Length; i++)
            {
                if (!_map.IsValidPosition(globalPositions[i])) continue;
                var chunkIndex = _map.FindChunkNumberByPosition(globalPositions[i]);
                var localPosition = new Vector3Int(globalPositions[i].x % ChunkData.ChunkSize,
                    globalPositions[i].y % ChunkData.ChunkSize,
                    globalPositions[i].z % ChunkData.ChunkSize);
                var currentBlock = _map.GetBlockByGlobalPosition(globalPositions[i]);
                if (currentBlock.Equals(blocks[i])) continue;
                if (!localPositionsByChunkIndex.ContainsKey(chunkIndex))
                {
                    localPositionsByChunkIndex[chunkIndex] = new List<Vector3Int>();
                }

                if (!colorsByChunkIndex.ContainsKey(chunkIndex))
                {
                    colorsByChunkIndex[chunkIndex] = new List<Color32>();
                }

                localPositionsByChunkIndex[chunkIndex].Add(localPosition);
                colorsByChunkIndex[chunkIndex].Add(blocks[i].Color);
                _mapGenerator.Chunks[chunkIndex].ChunkData.Blocks[
                    localPosition.x * ChunkData.ChunkSizeSquared + localPosition.y * ChunkData.ChunkSize +
                    localPosition.z] = new BlockData(blocks[i].Color);
            }

            foreach (var chunkIndex in localPositionsByChunkIndex.Keys)
            {
                UpdateChunkOnClient(chunkIndex, localPositionsByChunkIndex[chunkIndex], colorsByChunkIndex[chunkIndex]);
            }
        }
        

        [ClientRpc]
        private void UpdateChunkOnClient(int chunkIndex, List<Vector3Int> localPositions, List<Color32> colors)
        {
            _mapGenerator.Chunks[chunkIndex]
                .SpawnBlocks(localPositions, colors);
        }
        
    }
}