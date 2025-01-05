using VoxelMap;

namespace Networking.Messages.Responses
{
	public struct UpdateMapResponse : IMirrorResponse
	{
		public readonly Voxel[] Voxels;

		public UpdateMapResponse(Voxel[] voxels)
		{
			Voxels = voxels;
		}
	}
}