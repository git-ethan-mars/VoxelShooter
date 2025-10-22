using Data.SerializableDictionary;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Item Prefabs", menuName = "Inventory System/Item Prefabs")]
	public class ItemPrefabCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<ItemType, GameObject> ItemPrefabByItemType { get; private set; }
	}
}