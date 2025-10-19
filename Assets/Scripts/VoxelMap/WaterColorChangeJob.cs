using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	public struct WaterColorChangeJob : IJobFor
	{
		private MapData _mapData;
		private readonly Color32 _waterColor;

		public WaterColorChangeJob(MapData mapData, Color32 waterColor)
		{
			_mapData = mapData;
			_waterColor = waterColor;
		}

		public void Execute(int index)
		{
			int x = index / _mapData.Depth;
			int z = index % _mapData.Depth;

			if (!_mapData[x, 0, z].IsSolid())
			{
				_mapData[x, 0, z] = new VoxelData(_waterColor);
			}
		}
	}
}