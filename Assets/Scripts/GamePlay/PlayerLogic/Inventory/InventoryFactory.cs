using System;
using System.Linq;
using Common.AssetManagement;
using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class InventoryFactory : IInventoryFactory
	{
		private const string AssaultRiflePath = "Prefabs/Assault Rifle";
		private const string ShotgunPath = "Prefabs/Shotgun";
		private const string RiflePath = "Prefabs/Rifle";
		private const string MachineGunPath = "Prefabs/Machine Gun";
		private const string RevolverPath = "Prefabs/Revolver";
		private const string KnifePath = "Prefabs/Knife";
		private const string SpadePath = "Prefabs/Spade";
		private const string BlockPath = "Prefabs/Block";
		private const string RocketLauncherPath = "Prefabs/Rocket Launcher";
		private const string DrillLauncherPath = "Prefabs/Drill Launcher";
		private const string GrenadePath = "Prefabs/Grenade";
		private const string TntPath = "Prefabs/Tnt";
		
		private readonly IAssetProvider _assets;
		private readonly IStaticDataService _staticData;
		private readonly IEntityFactory _entityFactory;
		private readonly IInputService _inputService;

		public InventoryFactory(IAssetProvider assets, IStaticDataService staticData,
			IEntityFactory entityFactory, IInputService inputService)
		{
			_assets = assets;
			_staticData = staticData;
			_entityFactory = entityFactory;
			_inputService = inputService;
		}

		public InventorySystem CreateInventory(GameClass gameClass, MapProvider mapProvider)
		{
			var characteristic = _staticData.GetPlayerCharacteristic(gameClass);
			var rayCaster = new RayCaster(Camera.main, characteristic.placeDistance);
			var itemConfigures = _staticData.GetInventory(gameClass);
			var items = itemConfigures
				.Select(configure => configure.CreateModel(this, rayCaster, mapProvider))
				.ToList();
			var inventory = new InventorySystem(items);
			return inventory;
		}

		public RocketLauncher CreateRocketLauncher(RocketLauncherData data, RayCaster rayCaster, MapProvider mapProvider)
		{
			var rocketLauncher = _assets.Instantiate(RocketLauncherPath).GetComponent<RocketLauncher>();
			rocketLauncher.Construct(_inputService, _entityFactory, data, rayCaster, mapProvider);
			return rocketLauncher;
		}

		public RangeWeapon CreateRangeWeapon(RangeWeaponData data, RayCaster rayCaster)
		{
			RangeWeapon rangeWeapon = data.Type switch
			{
				RangeWeaponType.AssaultRifle => CreateAssaultRifle(),
				RangeWeaponType.Shotgun => CreateShotgun(),
				RangeWeaponType.Rifle => CreateRifle(),
				RangeWeaponType.MachineGun => CreateMachineGun(),
				RangeWeaponType.Revolver => CreateRevolver(),
				_ => throw new ArgumentOutOfRangeException(nameof(data.Type))
			};

			rangeWeapon.Construct(_inputService, data, rayCaster);

			return rangeWeapon;
		}

		private AssaultRifle CreateAssaultRifle()
		{
			var assaultRifle = _assets.Instantiate(AssaultRiflePath).GetComponent<AssaultRifle>();
			return assaultRifle;
		}

		private Shotgun CreateShotgun()
		{
			var assaultRifle = _assets.Instantiate(ShotgunPath).GetComponent<Shotgun>();
			return assaultRifle;
		}

		private Rifle CreateRifle()
		{
			var rifle = _assets.Instantiate(RiflePath).GetComponent<Rifle>();
			return rifle;
		}

		private MachineGun CreateMachineGun()
		{
			var machineGun = _assets.Instantiate(MachineGunPath).GetComponent<MachineGun>();
			return machineGun;
		}

		private Revolver CreateRevolver()
		{
			var revolver = _assets.Instantiate(RevolverPath).GetComponent<Revolver>();
			return revolver;
		}

		public MeleeWeapon CreateMeleeWeapon(MeleeWeaponData data, RayCaster rayCaster)
		{
			MeleeWeapon meleeWeapon;
			if (data.Type == MeleeWeaponType.Knife)
			{
				meleeWeapon = CreateKnife(data, rayCaster);
			}
			else
			{
				meleeWeapon = CreateSpade(data, rayCaster);
			}

			return meleeWeapon;
		}

		private Knife CreateKnife(MeleeWeaponData data, RayCaster rayCaster)
		{
			var knife = _assets.Instantiate(KnifePath).GetComponent<Knife>();
			knife.Construct(_inputService, data, rayCaster);
			return knife;
		}

		private Spade CreateSpade(MeleeWeaponData data, RayCaster rayCaster)
		{
			var spade = _assets.Instantiate(SpadePath).GetComponent<Spade>();
			spade.Construct(_inputService, data, rayCaster);
			return spade;
		}

		public Tnt CreateTnt(TntData data, RayCaster rayCaster)
		{
			var tnt = _assets.Instantiate(TntPath).GetComponent<Tnt>();
			tnt.Construct(_entityFactory, _assets, data, rayCaster);
			return tnt;
		}

		public Block CreateBlock(BlockData data, RayCaster rayCaster)
		{
			var block = _assets.Instantiate(BlockPath).GetComponent<Block>();
			block.Construct(_inputService, data, rayCaster);
			return block;
		}

		public Grenade CreateGrenade(GrenadeData data, RayCaster rayCaster)
		{
			var grenade = _assets.Instantiate(GrenadePath).GetComponent<Grenade>();
			grenade.Construct(_inputService, _entityFactory, data, rayCaster);
			return grenade;
		}

		public DrillLauncher CreateDrillLauncher(DrillLauncherData data, RayCaster rayCaster)
		{
			var drillLauncher = _assets.Instantiate(DrillLauncherPath).GetComponent<DrillLauncher>();
			drillLauncher.Construct(_inputService, _entityFactory, data, rayCaster);
			return drillLauncher;
		}
	}
}