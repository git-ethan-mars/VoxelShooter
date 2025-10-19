using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Inventory Collection", menuName = "Inventory System/Inventory Collection")]
	public class InventoryCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<GameClass, List<ItemType>> Inventory { get; private set; }
	}
}