using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
	public interface IBuildVisitor
	{
		bool Visit(Blueprint blueprint, RaycastHit rayCastHit, Color32 color);
		bool Build(IReadOnlyList<Vector3Int> positions, Color32 color);
		bool IsAvailablePosition(Vector3Int position);
	}
}
