using System;
using CameraLogic;
using Data;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using UnityEngine;
using Player = PlayerLogic.Player;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponModel : IInventoryItemModel
    {
        public event Action ModelUpdated;
        public int BulletsInMagazine
        {
            get => _rangeWeaponData.BulletsInMagazine;
            set
            {
                _rangeWeaponData.BulletsInMagazine = value;
                ModelUpdated?.Invoke();
            }
        }

        public int TotalBullets
        {
            get => _rangeWeaponData.TotalBullets;
            set
            {
                _rangeWeaponData.TotalBullets = value;
                ModelUpdated?.Invoke();
            }
        }

        private readonly RayCaster _rayCaster;
        private readonly RangeWeaponData _rangeWeaponData;
        private readonly Player _player;
        private readonly float _zoomMultiplier;

        public RangeWeaponModel(RayCaster rayCaster,
            RangeWeaponItem configure, RangeWeaponData rangeWeaponData, Player player)
        {
            _rayCaster = rayCaster;
            _rangeWeaponData = rangeWeaponData;
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
            foreach (var meshRenderer in _player.ItemPosition.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = false;
            }
        }

        public void ZoomOut()
        {
            _player.ZoomService.ZoomOut();
            foreach (var meshRenderer in _player.ItemPosition.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.enabled = true;
            }
        }
    }
}