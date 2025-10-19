using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Block Configure", menuName = "Inventory System/Item Configures/Block Configure")]
	public class BlockConfigure : InventoryItemConfigure
	{
		[field: SerializeField] public int Amount { get; private set; }
	}
}