using UnityEngine;

namespace VoxelMap
{
	public static class UIntExtensions
	{
		public static Color32 ToColor32(this uint packed)
		{
			byte a = (byte)(packed >> 24);
			byte r = (byte)(packed >> 16);
			byte g = (byte)(packed >> 8);
			byte b = (byte)(packed >> 0);
			return new Color32(r, g, b, a);
		}
	}
}
