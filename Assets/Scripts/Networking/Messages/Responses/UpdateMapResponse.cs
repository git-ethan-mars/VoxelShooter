using VoxelMap;

namespace Networking.Messages.Responses
{
	public struct UpdateMapResponse : IMirrorResponse
	{
		public readonly BlockDataWithPosition[] Blocks;

		public UpdateMapResponse(BlockDataWithPosition[] blocks)
		{
			Blocks = blocks;
		}
	}
}