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
			_hud = uiProvider.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = InventoryItem.Amount.Subscribe(OnAmountChanged);
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Select()
		{
			base.Select();

			_hud.ShowItemInfo(_projectileIcon, InventoryItem.Amount.ToString());
			_hud.SetCrosshairVisibility(true);
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.HideItemInfo();
			_hud.SetCrosshairVisibility(false);
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnAmountChanged(int amount)
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetItemCount(amount.ToString());
			}
		}
	}
}