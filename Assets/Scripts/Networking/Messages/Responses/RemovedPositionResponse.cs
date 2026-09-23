using System.Collections.Generic;
using VoxelMap;
namespace Networking.Messages
{
	public struct RemovedPositionResponse : IResponse
	{
		public readonly List<Vector3Ushort> RemovedPositions;

		public RemovedPositionResponse(List<Vector3Ushort> removedPositions)
		{
			RemovedPositions = removedPositions;
		}
	}
}