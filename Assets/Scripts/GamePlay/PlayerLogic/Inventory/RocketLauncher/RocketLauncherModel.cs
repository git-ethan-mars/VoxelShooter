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
            get => _rocketLauncherData.CarriedRockets;
            set
            {
                _rocketLauncherData.CarriedRockets = value;
                ModelUpdated?.Invoke();
            }
        }

        public int ChargedRockets
        {
            get => _rocketLauncherData.ChargedRockets;
            set
            {
                _rocketLauncherData.ChargedRockets = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly RocketLauncherData _rocketLauncherData;

        public RocketLauncherModel(RayCaster rayCaster, RocketLauncherData rocketLauncherData)
        {
            _rayCaster = rayCaster;
            _rocketLauncherData = rocketLauncherData;
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