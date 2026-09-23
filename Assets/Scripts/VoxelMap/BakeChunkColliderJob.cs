using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public struct BakeChunkColliderJob : IJob
	{
		private readonly EntityId _meshId;

		public BakeChunkColliderJob(EntityId meshId)
		{
			_meshId = meshId;
		}

		public void Execute()
		{
			Physics.BakeMesh(_meshId, false);
		}
	}
}