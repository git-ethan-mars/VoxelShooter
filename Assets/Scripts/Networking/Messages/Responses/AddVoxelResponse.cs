using System.Collections.Generic;
using VoxelMap;
namespace Networking.Messages
{
	public struct AddedVoxelResponse : IResponse
	{
		public readonly List<Voxel> AddedVoxels;

		public AddedVoxelResponse(List<Voxel> addedVoxels)
		{
			AddedVoxels = addedVoxels;
		}
	}
}