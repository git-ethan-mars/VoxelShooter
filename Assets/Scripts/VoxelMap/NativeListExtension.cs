using Unity.Collections;
namespace VoxelMap
{
	public static class NativeListExtension
	{
		public static void AddInt(this NativeList<byte> list, int value)
		{
			list.Add((byte)(value & 255));
			list.Add((byte)(value >> 8 & 255));
			list.Add((byte)(value >> 16 & 255));
			list.Add((byte)(value >> 24 & 255));
		}
	}
}