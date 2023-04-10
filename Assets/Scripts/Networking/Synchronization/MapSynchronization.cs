using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Networking.Synchronization
{
    public class MapSynchronization : NetworkBehaviour
    {
        private IMapProvider _mapProvider;
        private IMapGeneratorProvider _mapGeneratorProvider;
        private static byte[] CompressedMap { get; set; }

        private byte[] ClientBuffer { get; set; }

        private int DestinationOffset { get; set; }

        private const int PackageSize = 1024;

        public void Construct(IMapProvider mapProvider, IMapGeneratorProvider mapGeneratorProvider)
        {
            _mapProvider = mapProvider;
            _mapGeneratorProvider = mapGeneratorProvider;
        }


        public override void OnStartServer()
        {
            //CompressedMap = CustomNetworkManager.CompressedMap;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (isClientOnly)
                GetCompressedMapFromServer();
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
            _mapProvider.Map.MapData.SpawnPoints.Add(new SpawnPoint()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            });
            SendSpawnOnClients(position);
        }

        [ClientRpc]
        private void SendSpawnOnClients(Vector3Int position)
        {
            _mapProvider.Map.MapData.SpawnPoints.Add(new SpawnPoint()
            {
                X = position.x,
                Y = position.y,
                Z = position.z
            });
        }

        public void ChangeBlockState(List<Vector3Int> position, BlockData[] blocks)
        {
            UpdateBlocksOnServer(position, blocks.Select(block => block.Color).ToList());
        }


        [Command]
        private void UpdateBlocksOnServer(List<Vector3Int> globalPositions, List<Color32> colors)
        {
            var localPositionsByChunkIndex = new Dictionary<int, List<Vector3Int>>();
            var colorsByChunkIndex = new Dictionary<int, List<Color32>>();
            for (var i = 0; i < globalPositions.Count; i++)
            {
                if (!_mapProvider.Map.IsValidPosition(globalPositions[i])) continue;
                var chunkIndex = _mapProvider.Map.FindChunkNumberByPosition(globalPositions[i]);
                var localPosition = new Vector3Int(globalPositions[i].x % ChunkData.ChunkSize,
                    globalPositions[i].y % ChunkData.ChunkSize,
                    globalPositions[i].z % ChunkData.ChunkSize);
                var currentBlockColor =
                    _mapProvider.Map.MapData.Chunks[chunkIndex]
                        .Blocks[
                            localPosition.x * ChunkData.ChunkSizeSquared + localPosition.y * ChunkData.ChunkSize +
                            localPosition.z].Color;

                if (currentBlockColor.Equals(colors[i])) continue;
                if (!localPositionsByChunkIndex.ContainsKey(chunkIndex))
                {
                    localPositionsByChunkIndex[chunkIndex] = new List<Vector3Int>();
                }

                if (!colorsByChunkIndex.ContainsKey(chunkIndex))
                {
                    colorsByChunkIndex[chunkIndex] = new List<Color32>();
                }

                localPositionsByChunkIndex[chunkIndex].Add(localPosition);
                colorsByChunkIndex[chunkIndex].Add(colors[i]);
                _mapGeneratorProvider.MapGenerator.Chunks[chunkIndex].ChunkData.Blocks[
                    localPosition.x * ChunkData.ChunkSizeSquared + localPosition.y * ChunkData.ChunkSize +
                    localPosition.z] = new BlockData() {Color = colors[i]};
            }

            foreach (var chunkIndex in localPositionsByChunkIndex.Keys)
            {
                UpdateChunkOnClient(chunkIndex, localPositionsByChunkIndex[chunkIndex], colorsByChunkIndex[chunkIndex]);
            }
        }

        [Command]
        private void GetCompressedMapFromServer()
        {
            /*CompressedMap = CustomNetworkManager.CompressedMap;
            for (var i = 0; i < CompressedMap.Length; i += PackageSize)
            {
                ReceiveByteChunk(CompressedMap[i..Math.Min(i + PackageSize, CompressedMap.Length)],
                    CompressedMap.Length);
            }*/
        }

        [ClientRpc]
        private void UpdateChunkOnClient(int chunkIndex, List<Vector3Int> localPositions, List<Color32> colors)
        {
            _mapGeneratorProvider.MapGenerator.Chunks[chunkIndex]
                .SpawnBlocks(localPositions, colors);
        }


        [TargetRpc]
        private void ReceiveByteChunk(byte[] chunk, int allBytesCount)
        {
            Debug.Log($"Got chunk. Chunk length is : {chunk.Length}");
            ClientBuffer ??= new byte[allBytesCount];
            Buffer.BlockCopy(chunk, 0, ClientBuffer, DestinationOffset, chunk.Length);
            DestinationOffset += chunk.Length;
            if (DestinationOffset != allBytesCount) return;
            _mapProvider.Map = MapCompressor.Decompress(ClientBuffer);
            ClientBuffer = null;
            DestinationOffset = 0;
        }
    }
}