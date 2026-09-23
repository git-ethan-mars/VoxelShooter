using Data.SerializableDictionary;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Item Icons", menuName = "Inventory System/Item Icons")]
	public class ItemIconCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<ItemType, Sprite> SlotIconByItemType { get; private set; }
		[field: SerializeField] public SerializableDictionary<ItemType, Sprite> ProjectileIconByItemType { get; private set; }
		[field: SerializeField] public SerializableDictionary<ItemType, Sprite> ScopeIconByItemType { get; private set; }
	}

}