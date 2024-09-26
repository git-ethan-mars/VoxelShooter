using UnityEngine;

namespace Common.Extensions
{
	public static class Color32Extensions
	{
		public static bool IsEquals(this Color32 color, Color32 another)
		{
			return color.r == another.r && color.g == another.g && color.b == another.b && color.a == another.a;
		}
	}
}