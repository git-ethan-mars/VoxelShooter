using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Audio data")]
    public class AudioData : ScriptableObject
    {
        public AudioClip clip;

        [Range(0.0f, 1.0f)]
        public float volume;

        [Min(0.0f)]
        public float minDistance;

        [Min(0.0f)]
        public float maxDistance;
    }
}