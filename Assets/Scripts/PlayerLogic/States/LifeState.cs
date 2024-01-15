using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Services.StaticData;

namespace PlayerLogic.States
{
    public class LifeState : IPlayerState
    {
        private readonly PlayerData _playerData;
        private readonly IStaticDataService _staticData;

        public LifeState(PlayerData playerData, IStaticDataService staticData)
        {
            _playerData = playerData;
            _staticData = staticData;
        }

        public void Enter()
        {
            _playerData.IsAlive = true;
            _playerData.Characteristic = _staticData.GetPlayerCharacteristic(_playerData.GameClass);
            _playerData.Health = _playerData.Characteristic.maxHealth;
            _playerData.Items = _staticData.GetInventory(_playerData.GameClass).ToList();
            _playerData.ItemData = new List<IMutableItemData>();
            for (var i = 0; i < _playerData.Items.Count; i++)
            {
                var item = _playerData.Items[i];
                switch (item.itemType)
                {
                    case ItemType.RangeWeapon:
                    {
                        var rangeWeapon = (RangeWeaponItem) item;
                        _playerData.ItemData.Add(new RangeWeaponItemData(rangeWeapon));
                        break;
                    }
                    case ItemType.MeleeWeapon:
                        _playerData.ItemData.Add(new MeleeWeaponItemData());
                        break;
                    case ItemType.RocketLauncher:
                    {
                        var rocketLauncher = (RocketLauncherItem) item;
                        _playerData.ItemData.Add(new RocketLauncherItemData(rocketLauncher));
                        break;
                    }
                    case ItemType.Block:
                    {
                        var block = (BlockItem) item;
                        _playerData.ItemData.Add(new BlockItemData(block));
                        break;
                    }
                    case ItemType.Grenade:
                    {
                        var grenade = (GrenadeItem) item;
                        _playerData.ItemData.Add(new GrenadeItemData(grenade));
                        break;
                    }
                    case ItemType.Tnt:
                    {
                        var tnt = (TntItem) item;
                        _playerData.ItemData.Add(new TntItemData(tnt));
                        break;
                    }
                    case ItemType.Drill:
                    {
                        var drill = (DrillItem) item;
                        _playerData.ItemData.Add(new DrillItemData(drill));
                        break;
                    }
                }
            }
        }

        public void Exit()
        {
        }
    }
}