using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GamePlay
{
    public class MapSynchronization : NetworkBehaviour
    {
        private IGameFactory _gameFactory;
        private IMapProvider _mapProvider;
        private MapGenerator _mapGenerator;
        private static byte[] CompressedMap { get; set; }

        private byte[] ClientBuffer { get; set; }

        private int DestinationOffset { get; set; }

        private const int PackageSize = 1024;

        public void Construct(IGameFactory gameFactory, IMapProvider mapProvider, MapGenerator mapGenerator)
        {
            _gameFactory = gameFactory;
            _mapProvider = mapProvider;
            _mapGenerator = mapGenerator;
        }
        

        public override void OnStartServer()
        {
            //CompressedMap = CustomNetworkManager.CompressedMap;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            GlobalEvents.OnBlockChangeStateEvent.AddListener(ChangeBlockState);
            if (isServer)
            {
                GlobalEvents.SendMapLoadedState();
            }

            if (isClientOnly)
                GetCompressedMapFromServer();
        }


        private void ChangeBlockState(List<Vector3Int> position, BlockData[] blocks)
        {
            UpdateBlocksOnServer(position, blocks.Select(block => block.Color).ToList());
        }

        [Command]
        public void ApplyRaycast(Vector3 origin, Vector3 direction, float range, int damage)
        {
            var ray = new Ray(origin, direction);
            var raycastResult = Physics.Raycast(ray, out var rayHit, range);
            if (raycastResult)
            {
                var playerHealth = rayHit.collider.transform.parent.gameObject.GetComponent<HealthSystem>();
                if (playerHealth)
                {
                    playerHealth.Health -= damage;
                    if (playerHealth.Health <= 0)
                    {
                        Debug.Log("DEAD");
                    }
                }

                _gameFactory.CreateBulletHole(rayHit.point, Quaternion.Euler(rayHit.normal.y * -90,
                    rayHit.normal.x * 90 + rayHit.normal.z * -180, 0));
            }
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
                _mapGenerator.Chunks[chunkIndex].ChunkData.Blocks[
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
            _mapGenerator.Chunks[chunkIndex]
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
            GlobalEvents.SendMapLoadedState();
        }
    }
}