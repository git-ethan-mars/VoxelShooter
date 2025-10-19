using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class DrillLauncherPresenter : SlotPresenter<DrillLauncher>
	{
		private readonly Hud _hud;
		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public DrillLauncherPresenter(IStaticDataService staticData, UIProvider uiProvider,
			DrillLauncher drillLauncher, SlotView slotView) : base(staticData, drillLauncher, slotView)
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