using GamePlay;
using Services;

namespace UI.Inventory
{
	public class MeleeWeaponPresenter : SlotPresenter<MeleeWeapon>
	{
		private readonly Hud _hud;

		public MeleeWeaponPresenter(IStaticDataService staticData, UIProvider uiProvider, MeleeWeapon meleeWeapon, SlotView slotView) :
			base(staticData, meleeWeapon, slotView)
		{
			_hud = uiProvider.Hud;
		}

		public override void Select()
		{
			base.Select();

			_hud.SetCrosshairVisibility(true);
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.SetCrosshairVisibility(false);
		}
	}
}
