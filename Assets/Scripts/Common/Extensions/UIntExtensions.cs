using UnityEngine;

namespace Common.Extensions
{
	public static class UIntExtensions
	{
		public static Color32 ToColor32(this uint packed)
		{
			var a = (byte) (packed >> 24);
			var r = (byte) (packed >> 16);
			var g = (byte) (packed >> 8);
			var b = (byte) (packed >> 0);
			return new Color32(r, g, b, a);
		}
	}
}