using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public struct BakeChunkColliderJob : IJob
	{
		private readonly int _meshId;

		public BakeChunkColliderJob(int meshId)
		{
			_meshId = meshId;
		}

		public void Execute()
		{
			Physics.BakeMesh(_meshId, false);
		}
	}
}