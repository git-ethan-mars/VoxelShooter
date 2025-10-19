using UnityEngine;
using UnityEngine.Serialization;
namespace Data
{
	[CreateAssetMenu(fileName = "Voxel health", menuName = "Voxel Health Balance", order = 0)]
	public class VoxelHealthBalance : ScriptableObject
	{
		[FormerlySerializedAs("blockFullHealth")]
		[Min(0)]
		[SerializeField]
		private int voxelFullHealth;

		[FormerlySerializedAs("damagedBlockHealth")]
		[Min(0)]
		[SerializeField]
		private int damagedVoxel;

		[FormerlySerializedAs("wreckedVoxelHealth")]
		[Min(0)]
		[SerializeField]
		private int wreckedVoxel;

		[Range(0, 1)]
		[SerializeField]
		private float fullHealthColor;

		[Range(0, 1)]
		[SerializeField]
		private float damagedColor;

		[Range(0, 1)]
		[SerializeField]
		private float wreckedColor;

		public int VoxelFullHealth => voxelFullHealth;

		public int DamagedVoxelThreshold => damagedVoxel;

		public int WreckedVoxelThreshold => wreckedVoxel;

		public float FullHealthColorCoefficient => fullHealthColor;

		public float DamagedColor => damagedColor;

		public float WreckedColor => wreckedColor;
	}
}