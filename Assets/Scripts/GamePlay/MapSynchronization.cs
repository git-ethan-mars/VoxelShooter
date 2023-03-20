using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GamePlay
{
    public class MapSynchronization : NetworkBehaviour
    {
        private MapGenerator MapGenerator { get; set; }
        private static byte[] CompressedMap { get; set; }
        private Map Map { get; set; }

        private byte[] ClientBuffer { get; set; }
        private int DestinationOffset { get; set; }
        private const int PackageSize = 1024;

        private void Awake()
        {
            MapGenerator = GameObject.Find("MapGenerator").GetComponent<MapGenerator>();
        }

        public override void OnStartServer()
        {
            Map = CustomNetworkManager.Map;
            CompressedMap = CustomNetworkManager.CompressedMap;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            GlobalEvents.OnBlockChangeStateEvent.AddListener(ChangeBlockState);
            if (isServer)
            {
                MapGenerator.Initialize(Map);
                GlobalEvents.SendMapLoadedState();
            }

            if (isClientOnly)
                GetCompressedMapFromServer();
        }

        private void ChangeBlockState(List<Vector3Int> position, Block[] blocks)
        {
            Hack(position, blocks.Select(block => block.Color).ToList());
            //UpdateBlocksOnServer(position, blocks.Select(block => block.Color).ToList());
        }

        private void Hack(List<Vector3Int> globalPositions, List<Color32> colors)
        {
            var localPositionsByChunkIndex = new Dictionary<int, List<Vector3Int>>();
            var colorsByChunkIndex = new Dictionary<int, List<Color32>>();
            for (var i = 0; i < globalPositions.Count; i++)
            {
                if (!Map.IsValidPosition(globalPositions[i])) continue;
                var chunkIndex = Map.FindChunkNumberByPosition(globalPositions[i]);
                var localPosition = new Vector3Int(globalPositions[i].x % ChunkData.ChunkSize,
                    globalPositions[i].y % ChunkData.ChunkSize,
                    globalPositions[i].z % ChunkData.ChunkSize);
                var currentBlockColor =
                    Map.Chunks[chunkIndex]
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
                MapGenerator.Chunks[chunkIndex].ChunkData.Blocks[
                    localPosition.x * ChunkData.ChunkSizeSquared + localPosition.y * ChunkData.ChunkSize +
                    localPosition.z] = new Block() {Color = colors[i]};
            }

            foreach (var chunkIndex in localPositionsByChunkIndex.Keys)
            {
                MapGenerator.Chunks[chunkIndex]
                    .SpawnBlocks(localPositionsByChunkIndex[chunkIndex], colorsByChunkIndex[chunkIndex]);
            }
        }

        [Command]
        private void UpdateBlocksOnServer(List<Vector3Int> globalPositions, List<Color32> colors)
        {
            var localPositionsByChunkIndex = new Dictionary<int, List<Vector3Int>>();
            var colorsByChunkIndex = new Dictionary<int, List<Color32>>();
            for (var i = 0; i < globalPositions.Count; i++)
            {
                if (!Map.IsValidPosition(globalPositions[i])) continue;
                var chunkIndex = Map.FindChunkNumberByPosition(globalPositions[i]);
                var localPosition = new Vector3Int(globalPositions[i].x % ChunkData.ChunkSize,
                    globalPositions[i].y % ChunkData.ChunkSize,
                    globalPositions[i].z % ChunkData.ChunkSize);
                var currentBlockColor =
                    Map.Chunks[chunkIndex]
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
                MapGenerator.Chunks[chunkIndex].ChunkData.Blocks[
                    localPosition.x * ChunkData.ChunkSizeSquared + localPosition.y * ChunkData.ChunkSize +
                    localPosition.z] = new Block() {Color = colors[i]};
            }

            foreach (var chunkIndex in localPositionsByChunkIndex.Keys)
            {
                UpdateChunkOnClient(chunkIndex, localPositionsByChunkIndex[chunkIndex], colorsByChunkIndex[chunkIndex]);
            }
        }

        [Command]
        private void GetCompressedMapFromServer()
        {
            CompressedMap = CustomNetworkManager.CompressedMap;
            for (var i = 0; i < CompressedMap.Length; i += PackageSize)
            {
                ReceiveByteChunk(CompressedMap[i..Math.Min(i + PackageSize, CompressedMap.Length)],
                    CompressedMap.Length);
            }
        }

        [ClientRpc]
        private void UpdateChunkOnClient(int chunkIndex, List<Vector3Int> localPositions, List<Color32> colors)
        {
            MapGenerator.Chunks[chunkIndex]
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
            Map = MapCompressor.Decompress(ClientBuffer);
            ClientBuffer = null;
            DestinationOffset = 0;
            MapGenerator.Initialize(Map);
            GlobalEvents.SendMapLoadedState();
        }
    }
}