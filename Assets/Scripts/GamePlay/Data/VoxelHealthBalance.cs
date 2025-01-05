using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "Voxel health", menuName = "Voxel Health Balance", order = 0)]
    public class VoxelHealthBalance : ScriptableObject
    {
        [FormerlySerializedAs("blockFullHealth")]
        [Min(0)]
        [SerializeField]
        private int voxelFullHealth;

        public int VoxelFullHealth => voxelFullHealth;

        [FormerlySerializedAs("damagedBlockHealth")]
        [Min(0)]
        [SerializeField]
        private int damagedVoxel;

        public int DamagedVoxelThreshold => damagedVoxel;

        [FormerlySerializedAs("wreckedVoxelHealth")]
        [Min(0)]
        [SerializeField]
        private int wreckedVoxel;

        public int WreckedVoxelThreshold => wreckedVoxel;

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