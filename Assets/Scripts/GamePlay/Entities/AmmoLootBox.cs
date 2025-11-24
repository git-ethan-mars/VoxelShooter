using GamePlay.Core;
namespace GamePlay
{
	public class AmmoLootBox : LootBox
	{
		protected override void OnPickUp(Character character)
		{
			foreach (InventoryItem item in character.Inventory.Items)
			{
				switch (item)
				{
					case RangeWeapon rangeWeapon:
						rangeWeapon.TotalBullets.Value += rangeWeapon.Configure.MagazineSize * 2;
						break;
					case TNT tnt:
						tnt.Amount.Value += 1;
						break;
					case Grenade grenade:
						grenade.Amount.Value += 1;
						break;
					case DrillLauncher drillLauncher:
						drillLauncher.Amount.Value += 1;
						break;
					case RocketLauncher rocketLauncher:
						rocketLauncher.Amount.Value += 1;
						break;
				}
			} 
		}
	}
}