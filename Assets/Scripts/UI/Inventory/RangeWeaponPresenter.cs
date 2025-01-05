using GamePlay;

namespace UI.Inventory
{
	public class RangeWeaponPresenter : SlotPresenter
	{
		private readonly RangeWeapon _rangeWeapon;
		private readonly Hud _hud;

		public RangeWeaponPresenter(RangeWeapon rangeWeapon, SlotView slotView, Hud hud) : base(rangeWeapon, slotView)
		{
			_rangeWeapon = rangeWeapon;
			_hud = hud;
		}

		public override void Initialize()
		{
			base.Initialize();
			_rangeWeapon.Selected += Select;
			_rangeWeapon.Deselected += Deselect;
			_rangeWeapon.Data.TotalBulletsChanged += OnTotalBulletsChanged;
			_rangeWeapon.Data.BulletsInMagazineChanged += OnBulletsInMagazineChanged;
		}

		private void Select()
		{
			_hud.ShowAmmoInfo(_rangeWeapon.InventoryIcon,
				$"{_rangeWeapon.Data.BulletsInMagazine} / {_rangeWeapon.Data.TotalBullets}");
		}

		private void Deselect()
		{
			_hud.HideAmmoInfo();
		}

		private void OnTotalBulletsChanged(int totalBullets)
		{
			_hud.SetAmmoCount($"{_rangeWeapon.Data.BulletsInMagazine} / {totalBullets}");
		}

		private void OnBulletsInMagazineChanged(int bulletsInMagazine)
		{
			_hud.SetAmmoCount($"{bulletsInMagazine} / {_rangeWeapon.Data.TotalBullets}");
		}

		public override void Dispose()
		{
			base.Dispose();
			_rangeWeapon.Selected -= Select;
			_rangeWeapon.Deselected -= Deselect;
			_rangeWeapon.Data.TotalBulletsChanged -= OnTotalBulletsChanged;
			_rangeWeapon.Data.BulletsInMagazineChanged -= OnBulletsInMagazineChanged;
		}
	}
}