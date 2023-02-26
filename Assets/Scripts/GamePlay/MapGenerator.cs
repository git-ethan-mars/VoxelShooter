using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class MapGenerator : NetworkBehaviour
    {
        [SerializeField] private ChunkRenderer chunkPrefab;
        [SerializeField] private FloorRenderer floorRendererPrefab;
        private Map _map;
        private ChunkRenderer[] Chunks { get; set; }

        public void Start()
        {
            _map = MapReader.CreateNewMap();
            var floorRenderer = Instantiate(floorRendererPrefab, transform);
            floorRenderer.Map = _map;
            GlobalEvents.OnBlockChangeStateEvent.AddListener(ChangeBlockState);
            GlobalEvents.OnSaveMapEvent.AddListener(mapName => MapWriter.SaveMap(mapName, _map));
            Chunks = new ChunkRenderer[_map.Width / ChunkData.ChunkSize * _map.Height / ChunkData.ChunkSize *
                                       _map.Depth /
                                       ChunkData.ChunkSize];
            for (var x = 0; x < _map.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < _map.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < _map.Depth / ChunkData.ChunkSize; z++)
                    {
                        var index = z + y * _map.Depth / ChunkData.ChunkSize +
                                    x * _map.Height / ChunkData.ChunkSize * _map.Depth / ChunkData.ChunkSize;
                        var chunkRenderer = Instantiate(
                            chunkPrefab, new Vector3Int(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize,
                                z * ChunkData.ChunkSize), Quaternion.identity, transform);
                        Chunks[index] = chunkRenderer;
                        Chunks[index].ChunkData = _map.Chunks[index];
                    }
                }
            }
        }


        private void ChangeBlockState(Block block, Vector3Int position)
        {
            if (position.x >= 0 && position.x < _map.Width && position.y >= 0 && position.y < _map.Height &&
                position.z >= 0 && position.z < _map.Depth)
            {
                var chunkIndex = _map.FindChunkByPosition(position);
                var localPosition = new Vector3Int(position.x % ChunkData.ChunkSize, position.y % ChunkData.ChunkSize,
                    position.z % ChunkData.ChunkSize);
                UpdateChunkOnServer(chunkIndex, localPosition, block.ColorID);
            }
        }

        [Command(requiresAuthority = false)]
        private void UpdateChunkOnServer(int chunkIndex, Vector3Int localPosition, byte colorId)
        {
            UpdateChunkOnClient(chunkIndex, localPosition, colorId);
        }

        [ClientRpc]
        private void UpdateChunkOnClient(int chunkIndex, Vector3Int localPosition, byte colorId)
        {
            Chunks[chunkIndex].SpawnBlock(localPosition, new Block(){ColorID = colorId});
        }


        private void OnDestroy()
        {
            //MapWriter.SaveMap("a.rch", _map);
        }
    }
}