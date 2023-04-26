using System.Collections.Generic;
using Data;
using UnityEngine;

namespace MapLogic
{
    public class Map
    {
        public MapData MapData { get; }

        public Map(MapData mapData)
        {
            MapData = mapData;
        }


        public static Map CreateNewMap(int width = 512, int height = 64, int depth = 512)
        {
            var chunks = new ChunkData[width / ChunkData.ChunkSize * height / ChunkData.ChunkSize * depth /
                                       ChunkData.ChunkSize];
            for (var i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ChunkData();
            }
            var mapData = new MapData(chunks, width, height, depth, new List<SpawnPoint>());
            var map = new Map(mapData);
            //map.AddWater();
            return map;
        }

        private void AddWater()
        {
            var waterColor = new Color32();
            for (var x = 0; x < MapData.Width; x++)
            {
                for (var z = 0; z < MapData.Depth; z++)
                {
                    waterColor = MapData.Chunks[FindChunkNumberByPosition(x, 0, z)]
                        .Blocks[
                            (x & ChunkData.ChunkSize - 1) * ChunkData.ChunkSizeSquared +
                            (z & (ChunkData.ChunkSize - 1))].Color;
                }
            }

            if (waterColor.Equals(BlockColor.Empty))
                waterColor = new Color32(9, 20, 60, 255);

            for (var x = 0; x < MapData.Width; x++)
            {
                for (var z = 0; z < MapData.Depth; z++)
                {
                    var blocks = MapData.Chunks[FindChunkNumberByPosition(x, 0, z)].Blocks;
                    var block = blocks[
                        (x & (ChunkData.ChunkSize - 1)) * ChunkData.ChunkSizeSquared +
                        (z & (ChunkData.ChunkSize - 1))];
                    if (!block.Color
                            .Equals(BlockColor.Empty)) continue;
                    blocks[(x & (ChunkData.ChunkSize - 1)) * ChunkData.ChunkSizeSquared +
                           (z & (ChunkData.ChunkSize - 1))] = new BlockData(waterColor);
                }
            }
        }


        public BlockData GetBlockByGlobalPosition(int x, int y, int z)
        {
            return MapData.Chunks[FindChunkNumberByPosition(x,y,z)]
                .Blocks[
                    x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                                      y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize];
        }
        
        public BlockData GetBlockByGlobalPosition(Vector3Int position) =>
            GetBlockByGlobalPosition(position.x, position.y, position.z);

        public void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData) =>
            SetBlockByGlobalPosition(position.x, position.y, position.z, blockData);
        
        public void SetBlockByGlobalPosition(int x, int y, int z, BlockData blockData)
        {
            MapData.Chunks[FindChunkNumberByPosition(x, y, z)]
                .Blocks[
                    x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                    y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize] = blockData;
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
}