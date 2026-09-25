using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
	public static class VoxelLine
	{
		public static void GetPositions(Vector3Int start, Vector3Int end, int maxLength, List<Vector3Int> positions)
		{
			positions.Clear();
			Vector3Int delta = end - start;
			int steps = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
			int count = Mathf.Min(steps + 1, maxLength);

			for (int i = 0; i < count; i++)
			{
				float progress = steps == 0 ? 0.0f : (float)i / steps;
				positions.Add(start + Vector3Int.RoundToInt((Vector3)delta * progress));
			}
		}
	}
}
