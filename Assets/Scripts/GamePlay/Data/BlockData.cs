using System;
using UnityEngine;

namespace GamePlay.Data
{
	public class BlockData : IItemData
	{
		public int ID => _configure.id;
		public Sprite InventoryIcon => _configure.inventoryIcon;
		public Color32 SelectedColor { get; set; }

		public event Action<int> AmountChanged;

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

		private readonly BlockConfigure _configure;

		public BlockData(BlockConfigure configure)
		{
			_configure = configure;
			_amount = configure.count;
		}
	}
}