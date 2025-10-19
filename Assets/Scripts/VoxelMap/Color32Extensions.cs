using System.Runtime.CompilerServices;
using UnityEngine;
namespace VoxelMap
{
	public static class Color32Extensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual(this Color32 color, Color32 another)
		{
			return color.r == another.r && color.g == another.g && color.b == another.b && color.a == another.a;
		}
	}
}