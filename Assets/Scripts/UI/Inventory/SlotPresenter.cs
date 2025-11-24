using System;
using GamePlay.Core;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class SlotPresenter<TItem> : SlotPresenter where TItem : InventoryItem
	{
		protected readonly TItem InventoryItem;
		private readonly SlotView _slotView;

		protected readonly IStaticDataService StaticData;
		private Sprite _slotIcon;

		protected SlotPresenter(IStaticDataService staticData, TItem inventoryItem, SlotView slotView)
		{
			StaticData = staticData;
			InventoryItem = inventoryItem;
			_slotView = slotView;
		}

		public override void Initialize()
		{
			_slotIcon = StaticData.GetSlotIcon(InventoryItem.Type);
			_slotView.SetSlotIcon(_slotIcon);
		}

		public override void Select()
		{
			_slotView.Select();
		}

		public override void Deselect()
		{
			_slotView.Deselect();
		}

		public override void Dispose()
		{
			Deselect();
		}
	}

	public abstract class SlotPresenter : IDisposable
	{
		public abstract void Initialize();
		public abstract void Dispose();
		public abstract void Select();
		public abstract void Deselect();
	}
}