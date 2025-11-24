using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class RocketLauncherPresenter : SlotPresenter<RocketLauncher>
	{
		private readonly Hud _hud;
		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public RocketLauncherPresenter(IStaticDataService staticData, UIProvider uiProvider, RocketLauncher rocketLauncher, SlotView slotView) :
			base(staticData, rocketLauncher, slotView)
		{
			_hud = uiProvider.InGameUI.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = Observable.Merge(InventoryItem.ChargedRockets, InventoryItem.ChargedRockets)
				.Subscribe(_ => OnRocketsValueChanged());
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Select()
		{
			base.Select();

			_hud.ShowAmmoInfo(_projectileIcon, $"{InventoryItem.ChargedRockets} / {InventoryItem.Amount}");
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.HideAmmoInfo();
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnRocketsValueChanged()
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetAmmoCount($"{InventoryItem.ChargedRockets} / {InventoryItem.Amount}");
			}
		}
	}
}