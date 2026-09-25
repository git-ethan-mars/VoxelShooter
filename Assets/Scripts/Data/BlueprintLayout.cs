using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Blueprint Layout", menuName = "Inventory System/Blueprint Layout")]
	public class BlueprintLayout : ScriptableObject
	{
		[field: SerializeField] public string Name { get; private set; }
		[field: SerializeField] public List<Vector3Int> Positions { get; private set; }
		[field: SerializeField] public bool CanBuildLine { get; private set; }
	}
}
