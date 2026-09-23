using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class RangeWeaponPresenter : SlotPresenter<RangeWeapon>
	{
		private readonly Hud _hud;
		private readonly CameraProvider _cameraProvider;
		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public RangeWeaponPresenter(IStaticDataService staticData, UIProvider uiProvider, CameraProvider cameraProvider,
			SlotView slotView, RangeWeapon rangeWeapon) : base(staticData, rangeWeapon, slotView)
		{
			_hud = uiProvider.Hud;
			_cameraProvider = cameraProvider;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = Disposable.Combine(
				InventoryItem.TotalBullets.Subscribe(OnTotalBulletsChanged),
				InventoryItem.BulletsInMagazine.Subscribe(OnBulletsInMagazineChanged),
				InventoryItem.IsZoomed.Subscribe(OnWeaponZoomed));
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Select()
		{
			base.Select();

			_hud.ShowAmmoInfo(_projectileIcon, $"{InventoryItem.BulletsInMagazine} / {InventoryItem.TotalBullets}");
			_hud.SetCrosshairVisibility(true);
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.HideAmmoInfo();
			_hud.SetCrosshairVisibility(false);
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnTotalBulletsChanged(int totalBullets)
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetAmmoCount($"{InventoryItem.BulletsInMagazine} / {totalBullets}");
			}
		}

		private void OnBulletsInMagazineChanged(int bulletsInMagazine)
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetAmmoCount($"{bulletsInMagazine} / {InventoryItem.TotalBullets}");
			}
		}

		private void OnWeaponZoomed(bool isZoomed)
		{
			if (isZoomed)
			{
				_hud.SetScopeIcon(StaticData.GetScopeIcon(InventoryItem.Type));
				_hud.SetCrosshairVisibility(false);
				_cameraProvider.ZoomIn(InventoryItem.Configure.ZoomMultiplier);
			}
			else
			{
				_hud.SetScopeIcon(null);
				_hud.SetCrosshairVisibility(true);
				_cameraProvider.ZoomOut();
			}
		}
	}
}