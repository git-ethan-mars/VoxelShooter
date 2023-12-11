using CameraLogic;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponModel : IInventoryItemModel, IShooting, IReloading
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
        public ObservableVariable<int> TotalBullets { get; }

        private readonly RayCaster _rayCaster;
        private readonly Player _player;
        private readonly float _zoomMultiplier;

        public RangeWeaponModel(RayCaster rayCaster,
            RangeWeaponItem configure, RangeWeaponData data, Player player)
        {
            _rayCaster = rayCaster;
            _player = player;
            _zoomMultiplier = configure.zoomMultiplier;
            BulletsInMagazine = new ObservableVariable<int>(data.BulletsInMagazine);
            TotalBullets = new ObservableVariable<int>(data.TotalBullets);
        }

        public void ShootSingle()
        {
            NetworkClient.Send(new ShootRequest(_rayCaster.CentredRay, false));
        }

        public void ShootAutomatic()
        {
            NetworkClient.Send(new ShootRequest(_rayCaster.CentredRay, true));
        }

        public void CancelShoot()
        {
            NetworkClient.Send(new CancelShootRequest());
        }

        public void Reload()
        {
            NetworkClient.Send(new ReloadRequest());
        }

        public void ZoomIn()
        {
            _player.ZoomService.ZoomIn(_zoomMultiplier);
        }

        public void ZoomOut()
        {
            _player.ZoomService.ZoomOut();
        }
    }
}