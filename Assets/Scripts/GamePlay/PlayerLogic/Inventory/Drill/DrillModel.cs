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
            get => _drillLauncherData.Amount;
            set
            {
                _drillLauncherData.Amount = value;
                ModelUpdated?.Invoke();
            }
        }

        public int ChargedDrills
        {
            get => _drillLauncherData.ChargedDrills;
            set
            {
                _drillLauncherData.ChargedDrills = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly DrillLauncherData _drillLauncherData;

        public DrillModel(RayCaster rayCaster, DrillLauncherData rocketLauncherLauncherData)
        {
            _rayCaster = rayCaster;
            _drillLauncherData = rocketLauncherLauncherData;
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