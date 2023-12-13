using Data;
using UnityEngine;

namespace MapLogic
{
    public class MapProvider
    {
        public readonly string MapName;
        public readonly MapData MapData;
        public readonly MapSceneData SceneData;

        public MapProvider(MapData mapData, MapConfigure mapConfigure)
        {
            MapName = mapConfigure.name;
            MapData = mapData;
            SceneData = new MapSceneData(mapConfigure);
        }

        public BlockData GetBlockByGlobalPosition(Vector3Int position) =>
            GetBlockByGlobalPosition(position.x, position.y, position.z);

        public BlockData GetBlockByGlobalPosition(int x, int y, int z)
        {
            return MapData.Chunks[GetChunkNumberByGlobalPosition(x, y, z)]
                .Blocks[
                    x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                    y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize];
        }


        public int GetChunkNumberByGlobalPosition(int x, int y, int z)
        {
            return z / ChunkData.ChunkSize +
                   y / ChunkData.ChunkSize * (MapData.Depth / ChunkData.ChunkSize) +
                   x / ChunkData.ChunkSize *
                   (MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize);
        }

        public int GetChunkNumberByGlobalPosition(Vector3Int position)
        {
            return position.z / ChunkData.ChunkSize +
                   position.y / ChunkData.ChunkSize * (MapData.Depth / ChunkData.ChunkSize) +
                   position.x / ChunkData.ChunkSize *
                   (MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize);
        }

        public Vector3Int GetLocalPositionByGlobal(Vector3Int globalPosition)
        {
            return new Vector3Int(globalPosition.x % ChunkData.ChunkSize, globalPosition.y % ChunkData.ChunkSize,
                globalPosition.z % ChunkData.ChunkSize);
        }

        public bool IsInsideMap(int x, int y, int z)
        {
            return x >= 0 && x < MapData.Width &&
                   y >= 0 && y < MapData.Height &&
                   z >= 0 && z < MapData.Depth;
        }

        public bool IsDestructiblePosition(Vector3Int position)
        {
            return position.x >= 0 && position.x < MapData.Width &&
                   position.y > 0 && position.y < MapData.Height &&
                   position.z >= 0 && position.z < MapData.Depth;
        }

        public int GetChunkCount()
        {
            return MapData.Width / ChunkData.ChunkSize * MapData.Height / ChunkData.ChunkSize * MapData.Depth /
                   ChunkData.ChunkSize;
        }

        public int BlockCount => MapData.Width * MapData.Height * MapData.Depth;
    }
}