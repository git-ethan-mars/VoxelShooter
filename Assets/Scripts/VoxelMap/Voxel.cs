namespace VoxelMap
{
	public struct Voxel
	{
		public Vector3Ushort Position;
		public VoxelData Data;

		public Voxel(Vector3Ushort position, VoxelData data)
		{
			Position = position;
			Data = data;
		}
	}
}