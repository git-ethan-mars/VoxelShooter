using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class MapGenerator : NetworkBehaviour
    {
        [SerializeField] private ChunkRenderer chunkPrefab;
        [SerializeField] private FloorRenderer floorRendererPrefab;
        private Map Map { get; set; }
        private MapSynchronization MapSynchronization { get; set; }
        public ChunkRenderer[] Chunks { get; set; }

        public void Initialize(MapSynchronization mapSynchronization, Map map)
        {
            MapSynchronization = mapSynchronization;
            Map = map;
            var floorRenderer = Instantiate(floorRendererPrefab, transform);
            floorRenderer.Map = Map;
            GlobalEvents.OnBlockChangeStateEvent.AddListener(ChangeBlockState);
            GlobalEvents.OnSaveMapEvent.AddListener(mapName => MapWriter.SaveMap(mapName, Map));
            Chunks = new ChunkRenderer[Map.Width / ChunkData.ChunkSize * Map.Height / ChunkData.ChunkSize *
                                       Map.Depth /
                                       ChunkData.ChunkSize];
            for (var x = 0; x < Map.Width / ChunkData.ChunkSize; x++)
            {
                for (var y = 0; y < Map.Height / ChunkData.ChunkSize; y++)
                {
                    for (var z = 0; z < Map.Depth / ChunkData.ChunkSize; z++)
                    {
                        var index = z + y * Map.Depth / ChunkData.ChunkSize +
                                    x * Map.Height / ChunkData.ChunkSize * Map.Depth / ChunkData.ChunkSize;
                        var chunkRenderer = Instantiate(
                            chunkPrefab, new Vector3Int(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize,
                                z * ChunkData.ChunkSize), Quaternion.identity, transform);
                        Chunks[index] = chunkRenderer;
                        Chunks[index].ChunkData = Map.Chunks[index];
                    }
                }
            }
        }


        private void ChangeBlockState(Block block, Vector3Int position)
        {
            if (position.x >= 0 && position.x < Map.Width && position.y >= 0 && position.y < Map.Height &&
                position.z >= 0 && position.z < Map.Depth)
            {
                var chunkIndex = Map.FindChunkByPosition(position);
                var localPosition = new Vector3Int(position.x % ChunkData.ChunkSize, position.y % ChunkData.ChunkSize,
                    position.z % ChunkData.ChunkSize);
                MapSynchronization.UpdateChunkOnServer(chunkIndex, localPosition, block.ColorID);
            }
        }
    }
}