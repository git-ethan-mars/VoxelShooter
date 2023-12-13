using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Block health", menuName = "Block Health Balance", order = 0)]
    public class BlockHealthBalance : ScriptableObject
    {
        [Min(0)]
        [SerializeField]
        private int blockFullHealth;

        public int BlockFullHealth => blockFullHealth;

        [Min(0)]
        [SerializeField]
        private int damagedBlockHealth;

        public int DamagedBlockHealthThreshold => damagedBlockHealth;

        [Min(0)]
        [SerializeField]
        private int wreckedBlockHealth;

        public int WreckedBlockHealthThreshold => wreckedBlockHealth;

        [Range(0, 1)]
        [SerializeField]
        private float fullHealthColor;

        public float FullHealthColorCoefficient => fullHealthColor;

        [Range(0, 1)]
        [SerializeField]
        private float damagedColor;

        public float DamagedColor => damagedColor;

        [Range(0, 1)]
        [SerializeField]
        private float wreckedColor;

        public float WreckedColor => wreckedColor;
    }
}