using UnityEngine;

namespace GamePlay
{
	public static class LayerMasks
	{
		public static readonly LayerMask AttackMask = LayerMask.GetMask("Entity") | LayerMask.GetMask("Chunk");
		public static readonly LayerMask BuildMask = LayerMask.GetMask("Chunk");
		public static readonly LayerMask MovementBlockMask = LayerMask.GetMask("Entity") | LayerMask.GetMask("Chunk");
	}
}
