using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
	public static class VoxelLine
	{
		// Walks the voxels crossed by the segment between the voxel centres, one axis per step,
		// so every voxel shares a face with the previous one (no edge or corner-only contacts).
		public static void GetPositions(Vector3Int start, Vector3Int end, int maxLength, List<Vector3Int> positions)
		{
			positions.Clear();

			if (maxLength <= 0)
			{
				return;
			}

			Vector3Int delta = end - start;
			int stepCount = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z);
			var step = new Vector3Int(System.Math.Sign(delta.x), System.Math.Sign(delta.y), System.Math.Sign(delta.z));
			var nextBoundary = new Vector3(GetFirstCrossing(delta.x), GetFirstCrossing(delta.y), GetFirstCrossing(delta.z));
			var boundaryDistance = new Vector3(GetCrossingDistance(delta.x), GetCrossingDistance(delta.y), GetCrossingDistance(delta.z));
			Vector3Int current = start;
			positions.Add(current);

			for (int i = 0; i < stepCount && positions.Count < maxLength; i++)
			{
				if (nextBoundary.x <= nextBoundary.y && nextBoundary.x <= nextBoundary.z)
				{
					current.x += step.x;
					nextBoundary.x += boundaryDistance.x;
				}
				else if (nextBoundary.y <= nextBoundary.z)
				{
					current.y += step.y;
					nextBoundary.y += boundaryDistance.y;
				}
				else
				{
					current.z += step.z;
					nextBoundary.z += boundaryDistance.z;
				}

				positions.Add(current);
			}
		}

		// Fraction of the segment after which it leaves the start voxel along an axis.
		private static float GetFirstCrossing(int delta)
		{
			return delta == 0 ? float.PositiveInfinity : 0.5f / Mathf.Abs(delta);
		}

		// Fraction of the segment needed to cross one whole voxel along an axis.
		private static float GetCrossingDistance(int delta)
		{
			return delta == 0 ? float.PositiveInfinity : 1.0f / Mathf.Abs(delta);
		}
	}
}
