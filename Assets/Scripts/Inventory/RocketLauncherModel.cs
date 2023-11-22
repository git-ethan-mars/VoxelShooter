using CameraLogic;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;

namespace Inventory
{
    public class RocketLauncherModel : IInventoryItemModel, IConsumable
    {
        public ObservableVariable<int> Count { get; set; }
        private readonly RayCaster _rayCaster;


        public RocketLauncherModel(RayCaster rayCaster, RocketLauncherItem configure)
        {
            _rayCaster = rayCaster;
            Count = new ObservableVariable<int>(configure.count);
        }

        public void Shoot()
        {
            NetworkClient.Send(new RocketSpawnRequest(_rayCaster.CentredRay));
        }
    }
}