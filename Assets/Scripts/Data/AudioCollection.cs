using Data.SerializableDictionary;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Audio Collection", menuName = "Project Audio/Audio Collection")]
	public class AudioCollection : ScriptableObject
	{
		[field: SerializeField] public SerializableDictionary<AudioType, AudioData> AudioDataByType { get; private set; }
	}
}