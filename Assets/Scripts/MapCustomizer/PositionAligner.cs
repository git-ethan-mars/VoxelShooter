using UnityEngine;
using VoxelMap;
namespace MapCustomizer
{
	[ExecuteInEditMode]
	public class PositionAligner : MonoBehaviour
	{
		private void Update()
		{
			transform.position = Vector3Int.FloorToInt(transform.position) + Map.WorldOffset;
		}
	}
}