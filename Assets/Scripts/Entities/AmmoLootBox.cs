using Data;
using Mirror;
using Networking.Messages.Responses;

namespace Entities
{
    public class AmmoLootBox : LootBox
    {
        protected override void OnPickUp(NetworkConnectionToClient receiver)
        {
            var result = Server.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;

            for (var i = 0; i < playerData.Items.Count; i++)
            {
                var item = playerData.Items[i];
                var itemData = playerData.ItemData[i];
                if (item.itemType == ItemType.RangeWeapon)
                {
                    var rangeWeapon = (RangeWeaponItem) item;
                    var rangeWeaponData = (RangeWeaponItemData) itemData;
                    rangeWeaponData.TotalBullets += rangeWeapon.magazineSize * 2;
                    receiver.Send(new ReloadResultResponse(i, rangeWeaponData.TotalBullets,
                        rangeWeaponData.BulletsInMagazine));
                    continue;
                }

                if (item.itemType == ItemType.Tnt)
                {
                    var tntData = (TntItemData) itemData;
                    tntData.Amount += 1;
                    receiver.Send(new ItemUseResponse(i, tntData.Amount));
                    continue;
                }

                if (item.itemType == ItemType.Grenade)
                {
                    var grenadeData = (GrenadeItemData) itemData;
                    grenadeData.Amount += 1;
                    receiver.Send(new ItemUseResponse(i, grenadeData.Amount));
                    continue;
                }

                if (item.itemType == ItemType.RocketLauncher)
                {
                    var rocketLauncherData = (RocketLauncherItemData) itemData;
                    rocketLauncherData.CarriedRockets += 1;
                    receiver.Send(new RocketReloadResponse(i, rocketLauncherData.ChargedRockets,
                        rocketLauncherData.CarriedRockets));
                }
                
                if (item.itemType == ItemType.Drill)
                {
                    var drillData = (DrillItemData) itemData;
                    drillData.ChargedDrills += 1;
                    receiver.Send(new DrillReloadResponse(i, drillData.ChargedDrills,
                        drillData.Amount));
                }
            }
        }
    }
}