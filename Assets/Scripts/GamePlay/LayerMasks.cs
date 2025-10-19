using UnityEngine;
namespace GamePlay
{
	public static class LayerMasks
	{
		public static readonly LayerMask AttackMask = LayerMask.GetMask("Body") | LayerMask.GetMask("Chunk");
		public static readonly LayerMask BuildMask = LayerMask.GetMask("Chunk");
	}
}