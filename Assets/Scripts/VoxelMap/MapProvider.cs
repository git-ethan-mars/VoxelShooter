using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VoxelMap
{
	public class MapProvider
	{
		public int Width => _map.Data.Width;
		public int Height => _map.Data.Height;
		public int Depth => _map.Data.Depth;
		public int BlockCount => Width * Height * Depth;

		private readonly Map _map;

		public MapProvider(Map map)
		{
			_map = map;
		}

		public BlockData GetBlockByGlobalPosition(int x, int y, int z)
		{
			AssertPosition(x, y, z);
			var chunk = _map.Data.GetChunkByGlobalPosition(x, y, z);
			return chunk.GetBlock(x, y, z, PositionType.Global);
		}

		public BlockData GetBlockByGlobalPosition(Vector3Int position)
		{
			return GetBlockByGlobalPosition(position.x, position.y, position.z);
		}

		public void SetBlockByGlobalPosition(BlockDataWithPosition block)
		{
			AssertPosition(block.Position.x, block.Position.y, block.Position.z);
			_map.UpdateBlock(block);
			_map.ApplyChanges();
		}

		public void SetBlocksByGlobalPositions(List<BlockDataWithPosition> blocks)
		{
			for (var i = 0; i < blocks.Count; i++)
			{
				AssertPosition(blocks[i].Position.x, blocks[i].Position.y, blocks[i].Position.z);
			}

			_map.UpdateBlocks(blocks);
			_map.ApplyChanges();
		}

		public bool IsInsideMap(int x, int y, int z)
		{
			return x >= 0 && x < Width &&
			       y >= 0 && y < Height &&
			       z >= 0 && z < Depth;
		}

		public void WriteToStream(Stream stream)
		{
			MapWriter.WriteMap(_map.Data, stream);
		}

		private void AssertPosition(int x, int y, int z)
		{
			if (!IsInsideMap(x, y, z))
			{
				throw new ArgumentException($"{nameof(x)}={x} {nameof(y)}={y} {nameof(z)}={z} is not valid position");
			}
		}
	}
}