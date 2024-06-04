using System;
using Data;
using UnityEngine;

namespace MapLogic
{
    public class MapProvider : IMapProvider
    {
        public int Width => _mapData.Width;
        public int Height => _mapData.Height;
        public int Depth => _mapData.Depth;
        public Color32 WaterColor => _mapData.WaterColor;
        public Color32 SolidColor => _mapData.SolidColor;
        public int ChunkCount => _mapData.Chunks.Length;
        private readonly MapData _mapData;


        public MapProvider(MapData mapData)
        {
            _mapData = mapData;
        }

        public BlockData GetBlockByGlobalPosition(Vector3Int position) =>
            GetBlockByGlobalPosition(position.x, position.y, position.z);

        public BlockData GetBlockByGlobalPosition(int x, int y, int z)
        {
            AssertPosition(x, y, z);
            return _mapData.Chunks[GetChunkNumberByGlobalPosition(x, y, z)]
                .Blocks[x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                        y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize];
        }

        public void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData) =>
            SetBlockByGlobalPosition(position.x, position.y, position.z, blockData);

        public void SetBlockByGlobalPosition(int x, int y, int z, BlockData blockData)
        {
            AssertPosition(x, y, z);
            _mapData.Chunks[GetChunkNumberByGlobalPosition(x, y, z)].SetBlock(
                x % ChunkData.ChunkSize,
                y % ChunkData.ChunkSize,
                z % ChunkData.ChunkSize, blockData);
        }

        public int GetChunkNumberByGlobalPosition(Vector3Int position) =>
            GetChunkNumberByGlobalPosition(position.x, position.y, position.z);

        public Vector3Int GetLocalPositionByGlobal(Vector3Int globalPosition)
        {
            return new Vector3Int(globalPosition.x % ChunkData.ChunkSize, globalPosition.y % ChunkData.ChunkSize,
                globalPosition.z % ChunkData.ChunkSize);
        }

        public ChunkData GetChunkByIndex(int chunkIndex)
        {
            if (chunkIndex < 0 || chunkIndex > ChunkCount)
            {
                throw new ArgumentException($"Invalid chunk index: {chunkIndex}");
            }

            return _mapData.Chunks[chunkIndex];
        }

        public bool IsInsideMap(int x, int y, int z)
        {
            return x >= 0 && x < _mapData.Width &&
                   y >= 0 && y < _mapData.Height &&
                   z >= 0 && z < _mapData.Depth;
        }

        public bool IsDestructiblePosition(Vector3Int position)
        {
            return position.x >= 0 && position.x < _mapData.Width &&
                   position.y > 0 && position.y < _mapData.Height &&
                   position.z >= 0 && position.z < _mapData.Depth;
        }

        private int GetChunkNumberByGlobalPosition(int x, int y, int z)
        {
            if (!IsInsideMap(x, y, z))
            {
                throw new ArgumentException($"{x} {y} {z} is not valid position");
            }

            return z / ChunkData.ChunkSize +
                   y / ChunkData.ChunkSize * (_mapData.Depth / ChunkData.ChunkSize) +
                   x / ChunkData.ChunkSize *
                   (_mapData.Height / ChunkData.ChunkSize * _mapData.Depth / ChunkData.ChunkSize);
        }

        private void AssertPosition(int x, int y, int z)
        {
            if (!IsInsideMap(x, y, z))
            {
                throw new ArgumentException($"X={x} Y={y} Z={z} is not valid position");
            }
        }
    }
}