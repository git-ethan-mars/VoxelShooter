using System.Collections.Generic;
using Data;
using UnityEngine;

namespace MapLogic
{
    public class MapProvider : IMapProvider
    {
        public MapData MapData { get; }

        public MapProvider(MapData mapData)
        {
            MapData = mapData;
        }

        public static MapProvider CreateNewMap(int width = 512, int height = 64, int depth = 512)
        {
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }

            var mapData = new MapData(chunks, width, height, depth, new List<SpawnPointData>());
            var mapProvider = new MapProvider(mapData);
            return mapProvider;
        }


        public BlockData GetBlockByGlobalPosition(Vector3Int position) =>
            GetBlockByGlobalPosition(position.x, position.y, position.z);

        public BlockData GetBlockByGlobalPosition(int x, int y, int z)
        {
            return MapData.Chunks[FindChunkNumberByPosition(x, y, z)]
                .Blocks[
                    x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                    y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize];
        }


        public int FindChunkNumberByPosition(int x, int y, int z)
        {
            return z / ChunkData.ChunkSize +
                   y / ChunkData.ChunkSize * (MapData.Depth / ChunkData.ChunkSize) +
                   x / ChunkData.ChunkSize *
                   (MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize);
        }

        public int FindChunkNumberByPosition(Vector3Int position)
        {
            return position.z / ChunkData.ChunkSize +
                   position.y / ChunkData.ChunkSize * (MapData.Depth / ChunkData.ChunkSize) +
                   position.x / ChunkData.ChunkSize *
                   (MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize);
        }

        public bool IsValidPosition(Vector3Int globalPosition)
        {
            return !(globalPosition.x < 0 || globalPosition.x >= MapData.Width || globalPosition.y <= 0 ||
                     globalPosition.y >= MapData.Height ||
                     globalPosition.z < 0 || globalPosition.z >= MapData.Depth);
        }
    }

    public interface IMapProvider
    {
        BlockData GetBlockByGlobalPosition(Vector3Int position);
        BlockData GetBlockByGlobalPosition(int x, int y, int z);
        int FindChunkNumberByPosition(int x, int y, int z);
        int FindChunkNumberByPosition(Vector3Int position);
        bool IsValidPosition(Vector3Int globalPosition);
        MapData MapData { get; }
    }
}