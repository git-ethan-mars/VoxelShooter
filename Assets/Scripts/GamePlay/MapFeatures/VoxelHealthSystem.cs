using System;
using System.Collections.Generic;
using Data;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay.MapFeatures

{
	public class VoxelHealthSystem
	{
		private static readonly Color32 DestructedBlockColor = new Color32(0, 0, 0, 255);
		private readonly int _blockFullHealth;
		private readonly int _damagedBlockHealthThreshold;
		private readonly float _damagedColorCoefficient;
		private readonly int _depth;
		private readonly float _fullHealthColorCoefficient;
		private readonly int[] _healthByBlock;
		private readonly int _height;
		private readonly int _wreckedBlockHealthThreshold;
		private readonly float _wreckedColorCoefficient;

		public VoxelHealthSystem(Map map, IStaticDataService staticData)
		{
			_height = map.Height;
			_depth = map.Depth;
			VoxelHealthBalance healthBalance = staticData.GetVoxelHealthBalance();
			_blockFullHealth = healthBalance.VoxelFullHealth;
			_damagedBlockHealthThreshold = healthBalance.DamagedVoxelThreshold;
			_wreckedBlockHealthThreshold = healthBalance.WreckedVoxelThreshold;
			_fullHealthColorCoefficient = healthBalance.FullHealthColorCoefficient;
			_damagedColorCoefficient = healthBalance.DamagedColor;
			_wreckedColorCoefficient = healthBalance.WreckedColor;
			_healthByBlock = new int[map.Width * map.Height * map.Depth];
			Array.Fill(_healthByBlock, _blockFullHealth);
		}

		public void RestoreVoxels(List<Voxel> voxels)
		{
			for (var i = 0; i < voxels.Count; i++)
			{
				SetVoxelHealth(voxels[i], _blockFullHealth);
			}
		}

		public List<Voxel> DamageVoxels(List<Voxel> voxels, IVoxelDamageCalculator damageCalculator)
		{
			var changedVoxels = new List<Voxel>();
			for (var i = 0; i < voxels.Count; i++)
			{
				int previousHealth = GetVoxelHealth(voxels[i]);
				if (previousHealth == 0)
				{
					continue;
				}

				int currentHealth = Math.Max(previousHealth - damageCalculator.CalculateDamage(voxels[i]), 0);
				SetVoxelHealth(voxels[i], currentHealth);
				Color32 color = CalculateVoxelColor(voxels[i].Data, currentHealth, previousHealth);
				var damagedVoxel = new VoxelData(color);

				if (voxels[i].Data.Equals(damagedVoxel))
				{
					continue;
				}

				changedVoxels.Add(new Voxel(voxels[i].Position, damagedVoxel));
			}

			return changedVoxels;
		}

		private Color32 CalculateVoxelColor(VoxelData data, int currentHealth, int previousHealth)
		{
			if (currentHealth == 0)
			{
				return VoxelData.Air.Color;
			}
			
			if (currentHealth >= _damagedBlockHealthThreshold)
			{
				return Color32.Lerp(DestructedBlockColor, data.Color, _fullHealthColorCoefficient);
			}

			if (currentHealth >= _wreckedBlockHealthThreshold)
			{
				return Color32.Lerp(DestructedBlockColor, data.Color,
					previousHealth >= _damagedBlockHealthThreshold
						? _damagedColorCoefficient
						: _fullHealthColorCoefficient);
			}
			
			return Color32.Lerp(DestructedBlockColor, data.Color,
				previousHealth >= _wreckedBlockHealthThreshold
					? _wreckedColorCoefficient
					: _fullHealthColorCoefficient);
		}

		private int GetVoxelHealth(Voxel voxel)
		{
			return _healthByBlock[voxel.Position.x * _height * _depth +
			                      voxel.Position.y * _depth + voxel.Position.z];
		}

		private void SetVoxelHealth(Voxel voxel, int value)
		{
			_healthByBlock[voxel.Position.x * _height * _depth +
			               voxel.Position.y * _depth + voxel.Position.z] = value;
		}
	}
}