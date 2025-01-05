using GamePlay;

namespace UI.Inventory
{
	public class RocketLauncherPresenter : SlotPresenter
	{
		private readonly RocketLauncher _inventoryItem;
		private readonly Hud _hud;

		public RocketLauncherPresenter(RocketLauncher inventoryItem, SlotView slotView, Hud hud) : base(inventoryItem,
			slotView)
		{
			_inventoryItem = inventoryItem;
			_hud = hud;
		}

		public override void Initialize()
		{
			base.Initialize();
			_inventoryItem.Selected += OnSelected;
			_inventoryItem.Deselected += OnDeselected;
			_inventoryItem.Data.ChargedRocketsChanged += OnChargedRocketsChanged;
			_inventoryItem.Data.CarriedRocketsChanged += OnCarriedRocketsChanged;
		}

		public override void Dispose()
		{
			base.Dispose();
			_inventoryItem.Selected -= OnSelected;
			_inventoryItem.Deselected -= OnDeselected;
			_inventoryItem.Data.ChargedRocketsChanged -= OnChargedRocketsChanged;
			_inventoryItem.Data.CarriedRocketsChanged -= OnCarriedRocketsChanged;
		}

		private void OnSelected()
		{
			_hud.ShowAmmoInfo(_inventoryItem.InventoryIcon, $"{_inventoryItem.Data.ChargedRockets} / {_inventoryItem.Data.CarriedRockets}");
		}

		private void OnDeselected()
		{
			_hud.HideAmmoInfo();
		}

		private void OnChargedRocketsChanged(int chargedRockets)
		{
			_hud.SetAmmoCount($"{chargedRockets} / {_inventoryItem.Data.CarriedRockets}");
		}

		private void OnCarriedRocketsChanged(int carriedRockets)
		{
			_hud.SetAmmoCount($"{_inventoryItem.Data.ChargedRockets} / {carriedRockets}");
		}
	}
}