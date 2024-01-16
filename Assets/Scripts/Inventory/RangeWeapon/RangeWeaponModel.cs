using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponModel : IInventoryItemModel
    {
        public event Action ModelUpdated;
        public int BulletsInMagazine
        {
            get => _rangeWeaponItemData.BulletsInMagazine;
            set
            {
                _rangeWeaponItemData.BulletsInMagazine = value;
                ModelUpdated?.Invoke();
            }
        }

        public int TotalBullets
        {
            get => _rangeWeaponItemData.TotalBullets;
            set
            {
                _rangeWeaponItemData.TotalBullets = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly RangeWeaponItemData _rangeWeaponItemData;
        private readonly Player _player;
        private readonly float _zoomMultiplier;

        public RangeWeaponModel(RayCaster rayCaster,
            RangeWeaponItem configure, RangeWeaponItemData rangeWeaponItemData, Player player)
        {
            _rayCaster = rayCaster;
            _rangeWeaponItemData = rangeWeaponItemData;
            _player = player;
            _zoomMultiplier = configure.zoomMultiplier;
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
            _player.ItemPosition.gameObject.SetActive(false);
        }

        public void ZoomOut()
        {
            _player.ZoomService.ZoomOut();
            _player.ItemPosition.gameObject.SetActive(true);
        }
    }
}