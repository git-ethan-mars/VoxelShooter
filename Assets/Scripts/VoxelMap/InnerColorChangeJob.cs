using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public struct InnerColorChangeJob : IJobFor
	{
		private MapData _mapData;
		private readonly Color32 _newInnerColor;

		public InnerColorChangeJob(MapData mapData, Color32 newInnerColor)
		{
			_mapData = mapData;
			_newInnerColor = newInnerColor;
		}

		public void Execute(int chunkIndex)
		{
			for (var i = 0; i < Chunk.ChunkSizeCubed; i++)
			{
				int voxelIndex = chunkIndex * Chunk.ChunkSizeCubed + i;

				if (_mapData[voxelIndex] == VoxelData.DefaultInner)
				{
					_mapData[voxelIndex] = new VoxelData(_newInnerColor);
				}
			}
		}
	}
}