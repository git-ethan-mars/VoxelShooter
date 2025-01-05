using System;
using UnityEngine;

namespace GamePlay.Data
{
	public class DrillLauncherData : IItemData
	{
		public int ID => _drillLauncher.id;
		public Sprite InventoryIcon => _drillLauncher.inventoryIcon;
		public int Speed => _drillLauncher.speed;
		public int RotationSpeed => _drillLauncher.rotationSpeed;
		public float Lifetime => _drillLauncher.lifetime;
		public float ReloadTime => _drillLauncher.reloadTime;
		public AudioData ReloadSound => _drillLauncher.reloadSound;
		public int ChargedDrills { get; set; }
		public bool IsReloading { get; set; }

		public int Amount
		{
			get => _amount;
			set
			{
				_amount = value;
				AmountChanged?.Invoke(value);
			}
		}

		private int _amount;

		public event Action<int> AmountChanged;

		private readonly DrillLauncherConfigure _drillLauncher;

		public DrillLauncherData(DrillLauncherConfigure drillLauncher)
		{
			_drillLauncher = drillLauncher;
			_amount = drillLauncher.count;
			ChargedDrills = drillLauncher.chargedDrillsCapacity;
		}
	}
}