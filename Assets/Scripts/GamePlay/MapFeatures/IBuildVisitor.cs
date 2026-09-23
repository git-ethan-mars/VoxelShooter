using UnityEngine;

namespace GamePlay
{
	public interface IBuildVisitor
	{
		bool Visit(Block block, RaycastHit rayCastHit, Color32 color);
		bool Visit(Blueprint blueprint, RaycastHit rayCastHit, Color32 color);
		bool IsAvailablePosition(Vector3Int position);
	}
}
