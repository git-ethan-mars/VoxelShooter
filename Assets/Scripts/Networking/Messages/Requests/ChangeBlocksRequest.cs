using VoxelMap;

namespace Networking.Messages.Requests
{
	public struct AddBlocksRequest : IMirrorRequest
	{
		public readonly BlockDataWithPosition[] Blocks;

		public AddBlocksRequest(BlockDataWithPosition[] blocks)
		{
			Blocks = blocks;
		}
	}
}