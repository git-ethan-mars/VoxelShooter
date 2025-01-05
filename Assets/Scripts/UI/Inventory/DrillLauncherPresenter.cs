using GamePlay;

namespace UI.Inventory
{
	public class DrillLauncherPresenter : SlotPresenter
	{
		private readonly DrillLauncher _inventoryItem;
		private readonly Hud _hud;

		public DrillLauncherPresenter(DrillLauncher inventoryItem, SlotView slotView, Hud hud) : base(inventoryItem, slotView)
		{
			_inventoryItem = inventoryItem;
			_hud = hud;
		}

		public override void Initialize()
		{
			base.Initialize();
			_inventoryItem.Selected += OnSelected;
			_inventoryItem.Deselected += OnDeselected;
			_inventoryItem.Data.AmountChanged += OnAmountChanged;
		}

		private void OnSelected()
		{
			_hud.ShowItemInfo(_inventoryItem.InventoryIcon, _inventoryItem.Data.Amount.ToString());
		}

		private void OnDeselected()
		{
			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}

		public override void Dispose()
		{
			base.Dispose();
			_inventoryItem.Selected -= OnSelected;
			_inventoryItem.Deselected -= OnDeselected;
			_inventoryItem.Data.AmountChanged -= OnAmountChanged;
		}
	}
}