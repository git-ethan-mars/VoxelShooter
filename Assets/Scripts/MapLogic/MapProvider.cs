using System;
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

        public BlockData GetBlockByGlobalPosition(int x, int y, int z)
        {
            if (!IsInsideMap(x, y, z))
            {
                throw new ArgumentException($"{x} {y} {z} is not valid position");
            }

            return MapData.Chunks[GetChunkNumberByGlobalPosition(x, y, z)]
                .Blocks[x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                        y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize];
        }

        public BlockData GetBlockByGlobalPosition(Vector3Int position) =>
            GetBlockByGlobalPosition(position.x, position.y, position.z);

        public int GetChunkNumberByGlobalPosition(int x, int y, int z)
        {
            if (!IsInsideMap(x, y, z))
            {
                throw new ArgumentException($"{x} {y} {z} is not valid position");
            }

            return z / ChunkData.ChunkSize +
                   y / ChunkData.ChunkSize * (MapData.Depth / ChunkData.ChunkSize) +
                   x / ChunkData.ChunkSize *
                   (MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize);
        }

        public int GetChunkNumberByGlobalPosition(Vector3Int position) =>
            GetChunkNumberByGlobalPosition(position.x, position.y, position.z);

        public Vector3Int GetLocalPositionByGlobal(Vector3Int globalPosition)
        {
            return new Vector3Int(globalPosition.x % ChunkData.ChunkSize, globalPosition.y % ChunkData.ChunkSize,
                globalPosition.z % ChunkData.ChunkSize);
        }

        public bool TryGetFrontChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber + 1;
            if (notValidatedChunkNumber < ChunkCount &&
                chunkNumber / (MapData.Depth / ChunkData.ChunkSize) ==
                notValidatedChunkNumber / (MapData.Depth / ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public bool TryGetBackChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber - 1;
            if (notValidatedChunkNumber >= 0 && chunkNumber / (MapData.Depth / ChunkData.ChunkSize) ==
                notValidatedChunkNumber / (MapData.Depth / ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public bool TryGetUpChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber + MapData.Depth / ChunkData.ChunkSize;
            if (notValidatedChunkNumber < ChunkCount &&
                chunkNumber / (MapData.Depth / ChunkData.ChunkSize * MapData.Height /
                               ChunkData.ChunkSize) ==
                notValidatedChunkNumber /
                (MapData.Depth / ChunkData.ChunkSize * MapData.Height /
                 ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public bool TryGetDownChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber = chunkNumber - MapData.Depth / ChunkData.ChunkSize;
            if (notValidatedChunkNumber >= 0 &&
                chunkNumber / (MapData.Depth / ChunkData.ChunkSize * MapData.Height /
                               ChunkData.ChunkSize) ==
                notValidatedChunkNumber /
                (MapData.Depth / ChunkData.ChunkSize * MapData.Height /
                 ChunkData.ChunkSize))
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public bool TryGetRightChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber =
                chunkNumber + MapData.Height / ChunkData.ChunkSize * MapData.Depth / ChunkData.ChunkSize;
            if (notValidatedChunkNumber < ChunkCount)
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public bool TryGetLeftChunkNumber(int chunkNumber, out int neighbourChunkNumber)
        {
            var notValidatedChunkNumber =
                chunkNumber - MapData.Height / ChunkData.ChunkSize * MapData.Depth /
                ChunkData.ChunkSize;
            if (notValidatedChunkNumber >= 0)
            {
                neighbourChunkNumber = notValidatedChunkNumber;
                return true;
            }

            neighbourChunkNumber = -1;
            return false;
        }

        public int GetChunkXOffset(int chunkIndex)
        {
            return chunkIndex / (MapData.Height * MapData.Depth / ChunkData.ChunkSizeSquared) * ChunkData.ChunkSize;
        }
        
        public int GetChunkYOffset(int chunkIndex)
        {
            return chunkIndex / (MapData.Depth / ChunkData.ChunkSize) * ChunkData.ChunkSize;
        }
        
        public int GetChunkZOffset(int chunkIndex)
        {
            return chunkIndex % (MapData.Depth / ChunkData.ChunkSize) * ChunkData.ChunkSize;
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

        public int ChunkCount => MapData.Chunks.Length;

        public int BlockCount => MapData.Width * MapData.Height * MapData.Depth;
    }
}