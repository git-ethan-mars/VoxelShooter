using CameraLogic;
using Data;
using Infrastructure;
using Mirror;
using Networking.Messages.Requests;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponModel : IInventoryItemModel, IShooting, IReloading
    {
        public ObservableVariable<int> BulletsInMagazine { get; set; }
        public ObservableVariable<int> TotalBullets { get; set; }

        private int _bulletsInMagazine;
        private readonly RayCaster _rayCaster;
        private readonly float _zoomMultiplier;

        public RangeWeaponModel(RayCaster rayCaster, RangeWeaponData configure)
        {
            _rayCaster = rayCaster;
            _zoomMultiplier = configure.ZoomMultiplier;
            BulletsInMagazine = new ObservableVariable<int>(configure.BulletsInMagazine);
            TotalBullets = new ObservableVariable<int>(configure.TotalBullets);
        }

        public void ShootSingle()
        {
            NetworkClient.Send(new ShootRequest(_rayCaster.CentredRay, false));
        }

        public void ShootAutomatic()
        {
            NetworkClient.Send(new ShootRequest(_rayCaster.CentredRay, true));
        }

        public void Reload()
        {
            NetworkClient.Send(new ReloadRequest());
        }

        public void ZoomIn()
        {
            //_fpsCam.fieldOfView = Constants.DefaultFov / _zoomMultiplier;
        }

        public void ZoomOut()
        {
            //_fpsCam.fieldOfView = Constants.DefaultFov;
        }
    }
}