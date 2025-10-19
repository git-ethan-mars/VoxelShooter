using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class GrenadePresenter : SlotPresenter<Grenade>
	{
		private readonly Hud _hud;
		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public GrenadePresenter(IStaticDataService staticData, UIProvider uiProvider, Grenade grenade, SlotView slotView)
			: base(staticData, grenade, slotView)
		{
			_hud = uiProvider.InGameUI.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = InventoryItem.Amount.Subscribe(OnAmountChanged);
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		protected override void OnSelected()
		{
			base.OnSelected();

			_hud.ShowItemInfo(_projectileIcon, InventoryItem.Amount.ToString());
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();

			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}
	}
}