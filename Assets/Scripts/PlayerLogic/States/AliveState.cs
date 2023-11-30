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
            _playerData.CountByItem = new Dictionary<InventoryItem, int>();
            _playerData.Items = _staticData.GetInventory(_playerData.GameClass).ToList();
            _playerData.ItemData = new List<IMutableItemData>();
            _playerData.CountByItem = new Dictionary<InventoryItem, int>();
            foreach (var item in _playerData.Items)
            {
                if (item.itemType == ItemType.Tnt)
                {
                    _playerData.CountByItem[item] = ((TntItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.Grenade)
                {
                    _playerData.CountByItem[item] = ((GrenadeItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.Block)
                {
                    _playerData.CountByItem[item] = ((BlockItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    _playerData.CountByItem[item] = ((RocketLauncherItem) item).count;
                    continue;
                }

                _playerData.CountByItem[item] = 1;
            }

            for (var i = 0; i < _playerData.Items.Count; i++)
            {
                var item = _playerData.Items[i];
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var rangeWeapon = (RangeWeaponItem) item;
                    _playerData.ItemData.Add(new RangeWeaponData(rangeWeapon));
                    continue;
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    var meleeWeapon = (MeleeWeaponItem) item;
                    _playerData.ItemData.Add(new MeleeWeaponData(meleeWeapon));
                    continue;
                }

                _playerData.ItemData.Add(null);
            }
        }

        public void Exit()
        {
        }
    }
}