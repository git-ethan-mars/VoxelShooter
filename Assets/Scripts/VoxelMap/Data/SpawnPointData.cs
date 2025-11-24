using System;
using UnityEngine;
namespace VoxelMap.Data
{
	[Serializable]
	public class SpawnPointData
	{
		public Vector3Int position;

		public SpawnPointData(Vector3Int position)
		{
			this.position = position;
		}

		public Vector3 ToVectorWithOffset()
		{
			return position + new Vector3(0.5f, 0.5f, 0.5f);
		}
	}
}