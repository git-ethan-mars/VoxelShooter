using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Blueprint Configure", menuName = "Inventory System/Item Configures/Blueprint Configure")]
	public class BlueprintConfigure : InventoryItemConfigure
	{
		[field: SerializeField] public List<BlueprintLayout> Layouts { get; private set; }
	}
}
