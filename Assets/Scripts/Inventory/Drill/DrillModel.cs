using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;

namespace Inventory.Drill
{
    public class DrillModel : IInventoryItemModel
    {
        public event Action ModelUpdated;

        public int CarriedDrills
        {
            get => _drillItemData.Amount;
            set
            {
                _drillItemData.Amount = value;
                ModelUpdated?.Invoke();
            }
        }

        public int ChargedDrills
        {
            get => _drillItemData.ChargedDrills;
            set
            {
                _drillItemData.ChargedDrills = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly DrillItemData _drillItemData;

        public DrillModel(RayCaster rayCaster, DrillItemData rocketLauncherItemData)
        {
            _rayCaster = rayCaster;
            _drillItemData = rocketLauncherItemData;
        }

        public void Shoot()
        {
            NetworkClient.Send(new ShootRequest(_rayCaster.CentredRay, false));
        }

        public void Reload()
        {
            NetworkClient.Send(new ReloadRequest());
        }
    }
}