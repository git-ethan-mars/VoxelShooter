using System.Collections.Generic;
using UnityEngine;
using VoxelMap;

namespace UI
{
	public static class MapColorSampler
	{
		private const int SampleStep = 7;
		private const int ChannelBits = 4;
		private const int ChannelLevels = 1 << ChannelBits;
		private const int BucketCount = ChannelLevels * ChannelLevels * ChannelLevels;
		private const int MinColorDistanceSquared = 40 * 40;

		public static List<Color32> GetDominantColors(MapData.Readonly mapData, int count)
		{
			var voxelCounts = new int[BucketCount];
			var redSums = new long[BucketCount];
			var greenSums = new long[BucketCount];
			var blueSums = new long[BucketCount];

			for (int i = 0; i < mapData.VoxelCount; i += SampleStep)
			{
				Color32 color = mapData[i].Color;

				// Air and hidden inner voxels are transparent.
				if (color.a == 0)
				{
					continue;
				}

				int bucket = GetBucket(color);
				voxelCounts[bucket]++;
				redSums[bucket] += color.r;
				greenSums[bucket] += color.g;
				blueSums[bucket] += color.b;
			}

			var buckets = new List<int>();

			for (int i = 0; i < BucketCount; i++)
			{
				if (voxelCounts[i] > 0)
				{
					buckets.Add(i);
				}
			}

			buckets.Sort((first, second) => voxelCounts[second].CompareTo(voxelCounts[first]));

			var colors = new List<Color32>(count);

			foreach (int bucket in buckets)
			{
				if (colors.Count == count)
				{
					break;
				}

				int voxelCount = voxelCounts[bucket];
				var color = new Color32((byte)(redSums[bucket] / voxelCount), (byte)(greenSums[bucket] / voxelCount),
					(byte)(blueSums[bucket] / voxelCount), byte.MaxValue);

				if (IsDistinct(color, colors))
				{
					colors.Add(color);
				}
			}

			return colors;
		}

		private static int GetBucket(Color32 color)
		{
			int shift = 8 - ChannelBits;
			return (color.r >> shift) * ChannelLevels * ChannelLevels + (color.g >> shift) * ChannelLevels + (color.b >> shift);
		}

		private static bool IsDistinct(Color32 color, List<Color32> colors)
		{
			foreach (Color32 other in colors)
			{
				int red = color.r - other.r;
				int green = color.g - other.g;
				int blue = color.b - other.b;

				if (red * red + green * green + blue * blue < MinColorDistanceSquared)
				{
					return false;
				}
			}

			return true;
		}
	}
}
