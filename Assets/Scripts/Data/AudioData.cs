using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Audio data")]
	public class AudioData : ScriptableObject
	{
		[field: SerializeField] public AudioClip Clip { get; private set; }

		[field: Range(0.0f, 1.0f)]
		[field: SerializeField] public float Volume { get; private set; }

		[field: Min(0.0f)]
		[field: SerializeField] public float MinDistance { get; private set; }

		[field: Min(0.0f)]
		[field: SerializeField] public float MaxDistance { get; private set; }
	}
}