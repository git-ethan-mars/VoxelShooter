using System.Collections.Generic;
using UnityEngine;
using VoxelMap;

public class VerticalMapProjector
{
	public readonly Color32[] Projection;
	private readonly MapProvider _mapProvider;

	public VerticalMapProjector(MapProvider mapProvider)
	{
		_mapProvider = mapProvider;
		Projection = GetVerticalMapProjection();
	}

	private void OnMapUpdated(Voxel[] blocks)
	{
		var visitedPositions = new HashSet<(int, int)>();
		foreach (var block in blocks)
		{
			if (visitedPositions.Add((block.Position.x, block.Position.z)))
			{
				Projection[block.Position.z * _mapProvider.Width + block.Position.x] =
					GetHighestBlock(_mapProvider, block.Position.x, block.Position.z).Color;
			}
		}
	}

	private Color32[] GetVerticalMapProjection()
	{
		var projection = new Color32[_mapProvider.Width * _mapProvider.Depth];
		for (var x = 0; x < _mapProvider.Width; x++)
		{
			for (var z = 0; z < _mapProvider.Depth; z++)
			{
				projection[z * _mapProvider.Width + x] = GetHighestBlock(_mapProvider, x, z).Color;
			}
		}

		return projection;
	}

	private VoxelData GetHighestBlock(MapProvider mapProvider, int x, int z)
	{
		for (var y = mapProvider.Height - 1; y >= 0; y--)
		{
			var block = mapProvider.GetVoxelByGlobalPosition(x, y, z);
			if (!block.IsSolid())
			{
				continue;
			}

			return block;
		}

		return VoxelData.Air;
	}
}