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
            _playerData.ItemCountById = new Dictionary<int, int>();
            _playerData.ItemsId = _staticData.GetInventory(_playerData.GameClass).Select(item => item.id).ToList();
            _playerData.RangeWeaponsById = new Dictionary<int, RangeWeaponData>();
            _playerData.MeleeWeaponsById = new Dictionary<int, MeleeWeaponData>();
            _playerData.ItemCountById = new Dictionary<int, int>();
            foreach (var itemId in _playerData.ItemsId)
            {
                var item = _staticData.GetItem(itemId);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    _playerData.RangeWeaponsById[itemId] = new RangeWeaponData((RangeWeaponItem) item);
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    _playerData.MeleeWeaponsById[itemId] = new MeleeWeaponData((MeleeWeaponItem) item);
                }

                if (item.itemType == ItemType.Tnt)
                {
                    _playerData.ItemCountById[itemId] = ((TntItem) item).count;
                    continue;
                }
                
                if (item.itemType == ItemType.Grenade)
                {
                    _playerData.ItemCountById[itemId] = ((GrenadeItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.Block)
                {
                    _playerData.ItemCountById[itemId] = ((BlockItem) item).count;
                    continue;
                }
                
                if (item.itemType == ItemType.RocketLauncher)
                {
                    _playerData.ItemCountById[itemId] = ((RocketLauncherItem) item).count;
                    continue;
                }
                
                if (item.itemType == ItemType.Drill)
                {
                    _playerData.ItemCountById[itemId] = ((DrillItem) item).count;
                    continue;
                }

                _playerData.ItemCountById[itemId] = 1;
            }
        }

        public void Exit()
        {
        }
    }
}