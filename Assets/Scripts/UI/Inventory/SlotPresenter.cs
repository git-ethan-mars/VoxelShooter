using System;
using GamePlay.Core;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class SlotPresenter<TItem> : SlotPresenter where TItem : InventoryItem
	{
		protected readonly TItem InventoryItem;
		private readonly SlotView _slotView;
		private IDisposable _disposable;

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
			_disposable = InventoryItem.OnSelectStateChanged.Subscribe(OnItemSelected);

			_slotIcon = StaticData.GetSlotIcon(InventoryItem.Type);
			_slotView.SetSlotIcon(_slotIcon);
		}

		public override void Dispose()
		{
			OnDeselected();
			_disposable?.Dispose();
		}

		protected virtual void OnSelected()
		{
			_slotView.Select();
		}

		protected virtual void OnDeselected()
		{
			_slotView.Deselect();
		}

		private void OnItemSelected(bool isSelected)
		{
			if (isSelected)
			{
				OnSelected();
			}
			else
			{
				OnDeselected();
			}
		}
	}

	public abstract class SlotPresenter : IDisposable
	{
		public abstract void Initialize();
		public abstract void Dispose();
	}
}