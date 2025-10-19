using UnityEngine;
namespace GamePlay.MapFeatures
{
	public class BuildVisitorWrapper : MonoBehaviour, IBuildVisitor
	{
		private IBuildVisitor _visitor;

		public void Construct(IBuildVisitor visitor)
		{
			_visitor = visitor;
		}

		public void Visit(Block block, RaycastHit rayCastHit)
		{
			_visitor.Visit(block, rayCastHit);
		}
	}
}