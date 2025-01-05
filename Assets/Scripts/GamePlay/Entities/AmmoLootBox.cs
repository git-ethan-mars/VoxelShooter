namespace GamePlay.Entities
{
	public class AmmoLootBox : LootBox
	{
		protected override void OnPickUp(Character character)
		{
			character.Inventory.ApplyEffectToItems<RangeWeapon>(
				rangeWeapon => rangeWeapon.Data.TotalBullets += rangeWeapon.Data.MagazineSize * 2);
			character.Inventory.ApplyEffectToItems<Tnt>(tnt => tnt.Data.Amount += 1);
			character.Inventory.ApplyEffectToItems<Grenade>(grenade => grenade.Data.Amount += 1);
			character.Inventory.ApplyEffectToItems<RocketLauncher>(rocketLauncher => rocketLauncher.Data.CarriedRockets += 1);
			character.Inventory.ApplyEffectToItems<DrillLauncher>(drill=>drill.Data.ChargedDrills += 1);
		}
	}
}