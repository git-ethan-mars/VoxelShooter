using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;

namespace VoxelMap
{
	public class Map : MonoBehaviour
	{
		public Light DirectionalLight { get; private set; }
		private Color32 WaterColor { get; set; } = BlockData.Air.Color;
		private Color32 _innerColor = BlockData.Air.Color;

		public IReadOnlyList<Chunk> Chunks;
		private IReadOnlyList<GameObject> _walls;
		private GameObject _waterLayer;

		internal MapData Data;
		private readonly Dictionary<Chunk, List<BlockDataWithPosition>> _changesByChunk = new();

		public void Construct(MapData data)
		{
			Data = data;
		}

		public void SetFog(FogData fogData)
		{
			Environment.ApplyFog(fogData);
		}

		public void SetAmbient(AmbientData ambientData)
		{
			Environment.ApplyAmbientLighting(ambientData);
		}

		public void SetSkybox(Material skybox)
		{
			Environment.ApplySkybox(skybox);
		}

		public void SetWaterLayer(GameObject waterLayer)
		{
			_waterLayer = waterLayer;
			_waterLayer.GetComponent<MeshRenderer>().sharedMaterial.color = WaterColor;
		}

		public void SetWalls(GameObject[] walls)
		{
			_walls = walls;
		}

		public void SetWaterColor(Color32 color)
		{
			if (_waterLayer != null)
			{
				_waterLayer.GetComponent<MeshRenderer>().sharedMaterial.color = color;
			}

			WaterColor = color;

			for (var x = 0; x < Data.Width; x++)
			{
				for (var z = 0; z < Data.Depth; z++)
				{
					Data.GetChunkByGlobalPosition(x, 0, z).SetBlock(x, 0, z,
						new BlockData(WaterColor), PositionType.Global);
				}
			}
		}

		public void SetInnerColor(Color32 color)
		{
			var oldInnerColor = _innerColor;
			_innerColor = color;
			for (var i = 0; i < Data.ChunkCount; i++)
			{
				var chunk = Data.GetChunkDataByIndex(i);
				for (var j = 0; j < ChunkData.ChunkSizeCubed; j++)
				{
					var block = chunk.Blocks[j];
					if (block.Color.Equals(oldInnerColor))
					{
						chunk.Blocks[j] = new BlockData(_innerColor);
					}
				}
			}
		}

		public void SetDirectionalLight(Light directionalLight)
		{
			DirectionalLight = directionalLight;
		}

		public void UpdateBlocks(List<BlockDataWithPosition> blocks)
		{
			for (var i = 0; i < blocks.Count; i++)
			{
				UpdateBlock(blocks[i]);
			}
		}

		public void UpdateBlock(BlockDataWithPosition block)
		{
			var chunk = Chunks[Data.GetChunkNumberByGlobalPosition(block.Position.x, block.Position.y, block.Position.z)];
			if (!_changesByChunk.ContainsKey(chunk))
			{
				_changesByChunk[chunk] = new List<BlockDataWithPosition>();
			}

			_changesByChunk[chunk].Add(block);
		}

		public void ApplyChanges()
		{
			foreach (var (chunk, changes) in _changesByChunk)
			{
				chunk.SpawnBlocks(changes);
			}

			_changesByChunk.Clear();
		}

		public void SetChunks(Chunk[] chunks)
		{
			Chunks = chunks;
		}
	}
}