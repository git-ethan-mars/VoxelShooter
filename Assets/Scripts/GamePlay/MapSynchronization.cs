using System;
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
        private const int PackageSize = 4096;

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
            }

            if (isClientOnly)
                GetCompressedMapFromServer();
        }

        private void ChangeBlockState(Block block, Vector3Int position)
        {
            UpdateChunkOnServer(position, block.Color);
        }

        [Command]
        private void UpdateChunkOnServer(Vector3Int globalPosition, Color32 color)
        {
            if (globalPosition.x < 0 || globalPosition.x >= Map.Width || globalPosition.y <= 0 ||
                globalPosition.y >= Map.Height ||
                globalPosition.z < 0 || globalPosition.z >= Map.Depth) return;
            var chunkIndex = Map.FindChunkNumberByPosition(globalPosition);
            var localPosition = new Vector3Int(globalPosition.x % ChunkData.ChunkSize,
                globalPosition.y % ChunkData.ChunkSize,
                globalPosition.z % ChunkData.ChunkSize);
            var currentBlockColor =
                Map.Chunks[chunkIndex].Blocks[localPosition.x, localPosition.y, localPosition.z].Color;
            if (currentBlockColor.Equals(color))
                return;
            MapGenerator.Chunks[chunkIndex].ChunkData.Blocks[localPosition.x, localPosition.y, localPosition.z] =
                new Block() {Color = color};

            UpdateChunkOnClient(chunkIndex, localPosition, color);
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
        private void UpdateChunkOnClient(int chunkIndex, Vector3Int localPosition, Color32 color)
        {
            MapGenerator.Chunks[chunkIndex]
                .SpawnBlock(localPosition, new Block() {Color = color});
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
        }
    }
}