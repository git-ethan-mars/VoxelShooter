using System;
using Core;
using Mirror;
using UnityEngine;

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
            MapGenerator.Initialize(Map);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            GlobalEvents.onBlockChangeStateEvent.AddListener(ChangeBlockState);
            if (!isClientOnly) return;
            GetCompressedMapFromServer();
        }

        private void ChangeBlockState(Block block, Vector3Int position)
        {
            if (position.x < 0 || position.x >= Map.Width || position.y < 0 || position.y >= Map.Height ||
                position.z < 0 || position.z >= Map.Depth) return;
            var chunkIndex = Map.FindChunkByPosition(position);
            var localPosition = new Vector3Int(position.x % ChunkData.ChunkSize, position.y % ChunkData.ChunkSize,
                position.z % ChunkData.ChunkSize);
            UpdateChunkOnServer(chunkIndex, localPosition, block.ColorID);
        }

        [Command]
        private void UpdateChunkOnServer(int chunkIndex, Vector3Int localPosition, byte colorId)
        {
            if (Map.Chunks[chunkIndex].Blocks[localPosition.x, localPosition.y, localPosition.z].ColorID == colorId)
                return;
            UpdateChunkOnClient(chunkIndex, localPosition, colorId);
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
        private void UpdateChunkOnClient(int chunkIndex, Vector3Int localPosition, byte colorId)
        {
            MapGenerator.Chunks[chunkIndex].SpawnBlock(localPosition, new Block() {ColorID = colorId});
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