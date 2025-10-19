namespace GamePlay
{
	public class AmmoLootBox : LootBox
	{
		protected override void OnPickUp(Character character)
		{
			character.Inventory.ApplyEffectToItems<RangeWeapon>(
				rangeWeapon => rangeWeapon.TotalBullets.Value += rangeWeapon.Configure.MagazineSize * 2);
			character.Inventory.ApplyEffectToItems<TNT>(tnt => tnt.Amount.Value += 1);
			character.Inventory.ApplyEffectToItems<Grenade>(grenade => grenade.Amount.Value += 1);
			character.Inventory.ApplyEffectToItems<RocketLauncher>(rocketLauncher => rocketLauncher.Amount.Value += 1);
			character.Inventory.ApplyEffectToItems<DrillLauncher>(drill => drill.Amount.Value += 1);
		}
	}
}