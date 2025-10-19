using UnityEngine;
namespace GamePlay.MapFeatures
{
	public interface IBuildVisitor
	{
		void Visit(Block block, RaycastHit rayCastHit);
	}
}