using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;

namespace Inventory.RocketLauncher
{
    public class RocketLauncherModel : IInventoryItemModel
    {
        public event Action ModelUpdated;

        public int CarriedRockets
        {
            get => _rocketLauncherItemData.CarriedRockets;
            set
            {
                _rocketLauncherItemData.CarriedRockets = value;
                ModelUpdated?.Invoke();
            }
        }

        public int ChargedRockets
        {
            get => _rocketLauncherItemData.ChargedRockets;
            set
            {
                _rocketLauncherItemData.ChargedRockets = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly RocketLauncherItemData _rocketLauncherItemData;

        public RocketLauncherModel(RayCaster rayCaster, RocketLauncherItemData rocketLauncherItemData)
        {
            _rayCaster = rayCaster;
            _rocketLauncherItemData = rocketLauncherItemData;
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