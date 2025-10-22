using Data.SerializableDictionary;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Characteristics Collection", menuName = "Stats/Characteristics Collection")]
	public class CharacteristicsCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<GameClass, Characteristics> CharacteristicByItemType { get; private set; }
	}
}