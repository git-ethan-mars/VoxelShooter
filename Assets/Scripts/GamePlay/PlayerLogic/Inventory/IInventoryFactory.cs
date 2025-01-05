using Common;
using GamePlay.Data;
using VoxelMap;

namespace GamePlay
{
	public interface IInventoryFactory : IService
	{
		InventorySystem CreateInventory(GameClass gameClass, MapProvider mapProvider);
		RocketLauncher CreateRocketLauncher(RocketLauncherData data, RayCaster rayCaster, MapProvider mapProvider);
		RangeWeapon CreateRangeWeapon(RangeWeaponData data, RayCaster rayCaster);
		MeleeWeapon CreateMeleeWeapon(MeleeWeaponData data, RayCaster rayCaster);
		Tnt CreateTnt(TntData data, RayCaster rayCaster);
		Block CreateBlock(BlockData data, RayCaster rayCaster);
		Grenade CreateGrenade(GrenadeData data, RayCaster rayCaster);
		DrillLauncher CreateDrillLauncher(DrillLauncherData data, RayCaster rayCaster);
	}
}