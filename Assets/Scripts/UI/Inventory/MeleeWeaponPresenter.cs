using GamePlay;
using Services;
namespace UI.Inventory
{
	public class MeleeWeaponPresenter : SlotPresenter<MeleeWeapon>
	{
		public MeleeWeaponPresenter(IStaticDataService staticData, MeleeWeapon meleeWeapon, SlotView slotView) :
			base(staticData, meleeWeapon, slotView)
		{
		}
	}
}