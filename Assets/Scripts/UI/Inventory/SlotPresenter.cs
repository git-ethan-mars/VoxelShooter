using System;
using GamePlay;
using Services;

namespace UI.Inventory
{
	public class SlotPresenter<TItem> : SlotPresenter where TItem : InventoryItem
	{
		protected readonly TItem InventoryItem;

		protected readonly IStaticDataService StaticData;
		private readonly SlotView _slotView;

		protected SlotPresenter(IStaticDataService staticData, TItem inventoryItem, SlotView slotView)
		{
			StaticData = staticData;
			InventoryItem = inventoryItem;
			_slotView = slotView;
		}

		public override void Initialize()
		{
			_slotView.AddModel(InventoryItem);
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
			_slotView.Deselect();
			_slotView.RemoveModel();
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
