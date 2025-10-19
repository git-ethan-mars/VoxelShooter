namespace VoxelMap
{
	public static class FaceExtensions
	{
		public static bool HasFlag(this Face source, Face flag)
		{
			return (byte)(source & flag) == (byte)flag;
		}
	}
}