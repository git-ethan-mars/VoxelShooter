using Mirror;
using VoxelMap;
namespace Networking.Messages
{
	public struct UpdateMapResponse : NetworkMessage
	{
		public readonly Voxel[] Voxels;

		public UpdateMapResponse(Voxel[] voxels)
		{
			Voxels = voxels;
		}
	}
}