using CameraLogic;
using Data;
using Infrastructure;
using Infrastructure.Services.Storage;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UnityEngine;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponModel : IInventoryItemModel, IShooting, IReloading
    {
        public ObservableVariable<int> BulletsInMagazine { get; }
        public ObservableVariable<int> TotalBullets { get; }

        private readonly RayCaster _rayCaster;
        private readonly ZoomService _zoomService;
        private readonly float _aimSensitivity;
        private readonly int _sensitivity;
        private readonly Player _player;
        private int _bulletsInMagazine;

        public RangeWeaponModel(IStorageService storageService, RayCaster rayCaster, Camera camera,
            RangeWeaponData configure, Player player)
        {
            var mouseSettings = storageService.Load<MouseSettingsData>(Constants.MouseSettingKey);
            _aimSensitivity = mouseSettings.AimSensitivity;
            _sensitivity = mouseSettings.GeneralSensitivity;
            _rayCaster = rayCaster;
            _zoomService = new ZoomService(camera, configure.ZoomMultiplier);
            _player = player;
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
            _zoomService.ZoomIn();
            _player.Rotation.Sensitivity = _aimSensitivity;
        }

        public void ZoomOut()
        {
            _zoomService.ZoomOut();
            _player.Rotation.Sensitivity = _sensitivity;
        }
    }
}