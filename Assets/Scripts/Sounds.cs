using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


[CreateAssetMenu(fileName = "Sounds", menuName = "Audio/Sounds")]
public class Sounds : ScriptableObject
{
    [SerializeField] public TunedAudioClips[] AudioClips;

    [System.Serializable]
    public class TunedAudioClips
    {
        public AudioClip AudioClip;
        public int id;
        [Range(0, 1)] public float volume;
    }

    public TunedAudioClips GetAudioClip(int id)
    {
        return AudioClips.First(x => x.id == id);
    }
}
