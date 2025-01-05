using GamePlay;

namespace UI.Inventory
{
	public class GrenadePresenter : SlotPresenter
	{
		private readonly Grenade _grenade;
		private readonly Hud _hud;

		public GrenadePresenter(Grenade grenade, SlotView slotView, Hud hud) : base(grenade, slotView)
		{
			_grenade = grenade;
			_hud = hud;
		}

		public override void Initialize()
		{
			base.Initialize();
			_grenade.Selected += OnSelected;
			_grenade.Deselected += OnDeselected;
			_grenade.Data.AmountChanged += OnAmountChanged;
		}

		public override void Dispose()
		{
			base.Dispose();
			_grenade.Selected -= OnSelected;
			_grenade.Deselected -= OnDeselected;
			_grenade.Data.AmountChanged -= OnAmountChanged;
		}

		private void OnSelected()
		{
			_hud.ShowItemInfo(_grenade.InventoryIcon, _grenade.Data.Amount.ToString());
		}

		private void OnDeselected()
		{
			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}
	}
}