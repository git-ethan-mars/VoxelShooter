using System.Collections.Generic;
using Common;
using Common.AssetManagement;
using Common.StaticData;
using Entities;

namespace Inventory
{
	public interface IInventoryFactory : IService
	{
	}

	public class InventoryFactory : IInventoryFactory
	{
		private readonly IAssetProvider _assets;
		private readonly IStaticDataService _staticDataService;
		private readonly IEntityFactory _entityFactory;

		public InventoryFactory(IAssetProvider assets, IStaticDataService staticDataService, IEntityFactory entityFactory)
		{
			_assets = assets;
			_staticDataService = staticDataService;
			_entityFactory = entityFactory;
		}

		public Inventory CreateInventory(GameClass gameClass)
		{
			var itemConfigures = _staticDataService.GetInventory(gameClass);
			var items = new List<IInventoryItem>();
			foreach (var configure in itemConfigures)
			{
				switch (configure)
				{
					case RangeWeaponItem rangeWeapon:
					{
						items.Add(CreateRangeWeapon(new RangeWeaponData(rangeWeapon)));
						break;
					}
					case MeleeWeaponItem meleeWeapon:
						items.Add(CreateMeleeWeapon(new MeleeWeaponData(meleeWeapon)));
						break;
					case RocketLauncherItem rocketLauncher:
					{
						data.Add(new RocketLauncherData(rocketLauncher));
						break;
					}
					case BlockItem block:
					{
						data.Add(new BlockItemData(block));
						break;
					}
					case GrenadeItem grenade:
					{
						data.Add(new GrenadeData(grenade));
						break;
					}
					case TntItem tnt:
					{
						data.Add(new TntData(tnt));
						break;
					}
					case DrillLauncherItem drillLauncher:
					{
						data.Add(new DrillLauncherData(drillLauncher));
						break;
					}
				}
			}

			var inventory = _assets.Load<Inventory>("path");
			inventory.Construct(items);
			return inventory;
		}

		private RangeWeapon.RangeWeapon CreateRangeWeapon(RangeWeaponData data)
		{
			var rangeWeapon = _assets.Load<RangeWeapon.RangeWeapon>("path");
			rangeWeapon.Construct(data);
			return rangeWeapon;
		}
		
		private MeleeWeapon.MeleeWeapon CreateMeleeWeapon(MeleeWeaponData data)
		{
			var meleeWeapon = _assets.Load<MeleeWeapon.MeleeWeapon>("path");
			meleeWeapon.Construct(data);
			return meleeWeapon;
		}

		private GamePlay.PlayerLogic.Inventory.Tnt.Tnt CreateTnt(TntData data)
		{
			var tnt = _assets.Load<GamePlay.PlayerLogic.Inventory.Tnt.Tnt>("path");
			tnt.Construct(_entityFactory, _assets, data);
			return tnt;
		}
	}
}