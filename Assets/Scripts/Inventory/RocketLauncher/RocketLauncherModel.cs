using CameraLogic;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;

namespace Inventory.RocketLauncher
{
    public class RocketLauncherModel : IInventoryItemModel, ILaunching, IRocketReloadable
    {
        public ObservableVariable<int> Count { get; set; }
        public ObservableVariable<int> RocketsInSlotsCount { get; }
        private readonly RayCaster _rayCaster;
        
        public RocketLauncherModel(RayCaster rayCaster, RocketLauncherItem configure,
            RocketLauncherData rocketLauncherData)
        {
            _rayCaster = rayCaster;
            RocketsInSlotsCount  = new ObservableVariable<int>(rocketLauncherData.RocketsInSlotsCount);
            Count = new ObservableVariable<int>(rocketLauncherData.TotalRockets);
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