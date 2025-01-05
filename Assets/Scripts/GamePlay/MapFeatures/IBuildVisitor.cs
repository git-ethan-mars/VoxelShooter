using GamePlay.Data;
using UnityEngine;

namespace GamePlay.MapFeatures
{
	public interface IBuildVisitor
	{
		void Visit(BlockData block, RaycastHit rayCastHit);
	}
}