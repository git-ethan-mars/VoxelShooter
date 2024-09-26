using VoxelMap;

namespace Networking.Messages.Responses
{
	public struct FallBlockResponse : IMirrorResponse
	{
		public readonly BlockDataWithPosition[] Blocks;

		public FallBlockResponse(BlockDataWithPosition[] blocks)
		{
			Blocks = blocks;
		}
	}
}