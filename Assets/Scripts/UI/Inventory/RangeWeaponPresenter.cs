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
		private readonly CameraService _cameraService;
		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public RangeWeaponPresenter(IStaticDataService staticData, UIProvider uiProvider, CameraService cameraService,
			SlotView slotView, RangeWeapon rangeWeapon) : base(staticData, rangeWeapon, slotView)
		{
			_hud = uiProvider.InGameUI.Hud;
			_cameraService = cameraService;
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

		protected override void OnSelected()
		{
			base.OnSelected();

			_hud.ShowAmmoInfo(_projectileIcon, $"{InventoryItem.BulletsInMagazine} / {InventoryItem.TotalBullets}");
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();

			_hud.HideAmmoInfo();
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnTotalBulletsChanged(int totalBullets)
		{
			_hud.SetAmmoCount($"{InventoryItem.BulletsInMagazine} / {totalBullets}");
		}

		private void OnBulletsInMagazineChanged(int bulletsInMagazine)
		{
			_hud.SetAmmoCount($"{bulletsInMagazine} / {InventoryItem.TotalBullets}");
		}

		private void OnWeaponZoomed(bool isZoomed)
		{
			if (isZoomed)
			{
				_hud.ScopeImage.gameObject.SetActive(true);
				_hud.ScopeImage.sprite = StaticData.GetScopeIcon(InventoryItem.Type);
				_hud.CrosshairImage.gameObject.SetActive(false);
				_cameraService.ZoomIn(InventoryItem.Configure.ZoomMultiplier);
			}
			else
			{
				_hud.ScopeImage.gameObject.SetActive(false);
				_hud.ScopeImage.sprite = null;
				_hud.CrosshairImage.gameObject.SetActive(true);
				_cameraService.ZoomOut();
			}
		}
	}
}