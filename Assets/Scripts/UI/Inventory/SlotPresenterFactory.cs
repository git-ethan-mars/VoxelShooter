using System;
using GamePlay;

namespace UI.Inventory
{
	public class SlotPresenterFactory
	{
		private readonly Hud _hud;
		private readonly PaletteView _paletteView;
		public SlotPresenterFactory(Hud hud, PaletteView paletteView)
		{
			_hud = hud;
			_paletteView = paletteView;
		}

		public SlotPresenter CreatePresenter(InventoryItem item, SlotView slotView)
		{
			switch (item)
			{
				case Block block:
					return new BlockPresenter(block, slotView, _hud, _paletteView);
				case DrillLauncher drillLauncher:
					return new DrillLauncherPresenter(drillLauncher, slotView, _hud);
				case Grenade grenade:
					return new GrenadePresenter(grenade, slotView, _hud);
				case MeleeWeapon meleeWeapon:
					return new MeleeWeaponPresenter(meleeWeapon, slotView);
				case RangeWeapon rangeWeapon:
					return new RangeWeaponPresenter(rangeWeapon, slotView, _hud);
				case RocketLauncher rocketLauncher:
					return new RocketLauncherPresenter(rocketLauncher, slotView, _hud);
				case Tnt tnt:
					return new TntPresenter(tnt, slotView, _hud);
				default:
					throw new ArgumentOutOfRangeException(nameof(item));
			}
		}
	}
}