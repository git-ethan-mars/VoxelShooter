using System.Collections.Generic;
using VoxelMap;

namespace Networking.Messages
{
	public struct FallingVoxelsResponse : IResponse
	{
		public readonly List<Voxel> Voxels;

		public FallingVoxelsResponse(List<Voxel> voxels)
		{
			Voxels = voxels;
		}
	}
}
