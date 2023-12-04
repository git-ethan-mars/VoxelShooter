using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Lobby balance", menuName = "", order = 0)]
    public class LobbyBalance : ScriptableObject
    {
        [Header("Box spawn time")]
        [Min(1)]
        public int boxSpawnTime;

        [Header("Spawn time")]
        [Min(1)]
        public int spawnTime;

        [Header("Match duration")]
        [Min(1)]
        public int minMatchDuration;

        [Min(1)]
        public int maxMatchDuration;
    }
}