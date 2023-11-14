using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Lobby balance", menuName = "", order = 0)]
    public class LobbyBalance : ScriptableObject
    {
        [Header("Box spawn time")]
        [Min(0)]
        public int minBoxSpawnTime;

        [Min(0)]
        public int maxBoxSpawnTime;

        [Header("Spawn time")]
        [Min(0)]
        public int minSpawnTime;

        [Min(0)]
        public int maxSpawnTime;

        [Header("Match duration")]
        [Min(0)]
        public int minMatchDuration;

        [Min(0)]
        public int maxMatchDuration;
    }
}