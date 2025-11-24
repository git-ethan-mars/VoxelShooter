using System;
using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;

namespace GamePlay.MapFeatures
{
	public class VoxelHealthSystem : MapFeature
	{
		private const int MaxHealth = 50;

		private readonly Dictionary<Vector3Ushort, float> _health = new Dictionary<Vector3Ushort, float>();
		private MapProvider _mapProvider;
		private IDisposable _disposable;

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
		}

		public void ApplyDamage(IReadOnlyList<Voxel> voxels, IVoxelDamageCalculator damageCalculator)
		{
			using var poolObject = ListPool<Voxel>.Get(out var changedVoxels);

			for (var i = 0; i < voxels.Count; i++)
			{
				Vector3Ushort position = voxels[i].Position;

				if (!_mapProvider.Map.IsInsideMap(position.x, position.y, position.z))
				{
					return;
				}

				if (!_mapProvider.Map.GetVoxelByGlobalPosition(position).IsSolid())
				{
					return;
				}

				float current = _health.GetValueOrDefault(position, MaxHealth);

				Color.RGBToHSV(voxels[i].Data.Color, out float hue, out float saturation, out float brightness);
				float maxBrightness = brightness / current * MaxHealth;

				current -= damageCalculator.CalculateDamage(voxels[i]);

				if (current <= 0)
				{
					_health.Remove(position);

					changedVoxels.Add(new Voxel(position, VoxelData.Air));
				}
				else
				{
					_health[position] = current;

					float currentBrightness = maxBrightness * (current / MaxHealth + 1) / 2;
					Color newColor = Color.HSVToRGB(hue, saturation, currentBrightness);

					changedVoxels.Add(new Voxel(position, new VoxelData(newColor)));
				}
			}

			_mapProvider.Map.SetVoxelsByGlobalPositions(changedVoxels);
		}
	}
}