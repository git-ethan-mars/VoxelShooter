using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Blueprint Configure", menuName = "Inventory System/Item Configures/Blueprint Configure")]
	public class BlueprintConfigure : InventoryItemConfigure
	{
		[field: Min(1)]
		[field: SerializeField]
		public int MaxLineLength { get; private set; } = 32;
	}
}
