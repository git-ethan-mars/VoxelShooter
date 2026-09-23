using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace VoxelMap
{
	[BurstCompile]
	public readonly struct BakeChunkColliderParallelJob : IJobFor
	{
		[ReadOnly]
		private readonly NativeArray<EntityId> _meshIndexes;

		public BakeChunkColliderParallelJob(NativeArray<EntityId> meshIndexes)
		{
			_meshIndexes = meshIndexes;
		}

		public void Execute(int index)
		{
			Physics.BakeMesh(_meshIndexes[index], false);
		}
	}
}