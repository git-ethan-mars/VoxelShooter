using Data.SerializableDictionary;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Item Configure Collection", menuName = "Inventory System/Item Configure Collection")]
	public class ItemConfigureCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<ItemType, InventoryItemConfigure> ConfigureByItemType { get; private set; }
	}
}