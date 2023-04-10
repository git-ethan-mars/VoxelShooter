using System;
using System.Collections;
using Data;
using Infrastructure;
using Infrastructure.Factory;
using Networking.Synchronization;
using TMPro;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Inventory
{
    public class GunSystem : IInventoryItemView, ILeftMouseButtonDownHandler, ILeftMouseButtonHoldHandler, IUpdated
    {
        public Sprite Icon { get; }
        public GameObject Model { get; }
        public float RecoilModifier;
        public readonly string Guid;
        public readonly float StepRecoil;
        public int BulletsInMagazine;
        public readonly int MagazineSize;
        public bool _readyToShoot;
        public int TotalBullets;
        public int BulletsPerShot;
        private readonly PrimaryWeapon _weaponConfiguration;
        private readonly TextMeshProUGUI _ammoInfo;
        private readonly Transform _attackPoint;
        private bool _reloading;
        private readonly Camera _fpsCam;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IGameFactory _gameFactory;
        private readonly RaycastSynchronization _raycastSynchronization;
        private readonly StatSynchronization _statSynchronization;

        public GunSystem(IGameFactory gameFactory, ICoroutineRunner coroutineRunner, Camera camera,
            Transform itemPosition, GameObject player, GameObject hud, PrimaryWeapon configuration)
        {
            _gameFactory = gameFactory;
            _coroutineRunner = coroutineRunner;
            _weaponConfiguration = configuration;
            Model = Object.Instantiate(_weaponConfiguration.prefab,
                itemPosition);
            Model.SetActive(false);
            Icon = _weaponConfiguration.icon;
            _attackPoint = Model.GetComponentInChildren<Transform>();
            _fpsCam = camera;
            _ammoInfo = hud.GetComponent<Hud>().ammoCount.GetComponent<TextMeshProUGUI>();
            BulletsInMagazine = _weaponConfiguration.magazineSize;
            _readyToShoot = true;
            _raycastSynchronization = player.GetComponent<RaycastSynchronization>();
            MagazineSize = _weaponConfiguration.magazineSize;
            Guid = Convert.ToString(System.Guid.NewGuid());
            StepRecoil = _weaponConfiguration.stepRecoil;
            _statSynchronization = player.GetComponent<StatSynchronization>();
        }

        public void Select()
        {
            _ammoInfo.gameObject.SetActive(true);
            Model.SetActive(true);
        }


        public void Unselect()
        {
            _ammoInfo.gameObject.SetActive(false);
            Model.SetActive(false);
        }

        public void InnerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R) && BulletsInMagazine < _weaponConfiguration.magazineSize && !_reloading)
                Reload();
            _ammoInfo.SetText($"{BulletsInMagazine} / {_weaponConfiguration.magazineSize}");
        }

        public void OnLeftMouseButtonDown()
        {
            if (_weaponConfiguration.isAutomatic)
                return;
            ReadyToShoot();
        }

        public void OnLeftMouseButtonHold()
        {
            if (!_weaponConfiguration.isAutomatic)
                return;
            ReadyToShoot();
        }

        private void ReadyToShoot()
        {
            if (_readyToShoot && !_reloading && BulletsInMagazine > 0)
            {
                BulletsPerShot = _weaponConfiguration.bulletsPerTap;
                Shoot();
            }

            _ammoInfo.SetText($"{BulletsInMagazine} / {_weaponConfiguration.magazineSize}");
        }

        private void Shoot()
        {
            var x = Math.Abs(RecoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * RecoilModifier;
            var y = Math.Abs(RecoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * RecoilModifier;

            var direction = new Vector3(0.5f, 0.5f);
            if (_weaponConfiguration.isAutomatic)
                direction += new Vector3(x, y);

            var ray = _fpsCam.ViewportPointToRay(direction);
            _raycastSynchronization.ApplyRaycast(ray.origin, ray.direction, _weaponConfiguration.range,
                _weaponConfiguration.damage);

            _gameFactory.CreateMuzzleFlash(_attackPoint);

            _statSynchronization.OnShoot(Guid);
        }

        public void StartShootCoroutines()
        {
            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetShoot, _weaponConfiguration.timeBetweenShooting));
            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetRecoil, _weaponConfiguration.resetTimeRecoil));
            if (BulletsPerShot > 0 && BulletsInMagazine > 0)
                _coroutineRunner.StartCoroutine(WaitForSeconds(Shoot, _weaponConfiguration.timeBetweenShots));
        }

        private void ResetRecoil()
        {
            RecoilModifier -= _weaponConfiguration.stepRecoil;
        }

        private void ResetShoot()
        {
            _readyToShoot = true;
        }

        private void Reload()
        {
            _reloading = true;
            _statSynchronization.OnReload(Guid);
            StartReloadCoroutine();
        }

        public void StartReloadCoroutine()
        {
            _coroutineRunner.StartCoroutine(WaitForSeconds(ReloadFinished, _weaponConfiguration.reloadTime));
        }

        private void ReloadFinished()
        {
            BulletsInMagazine = _weaponConfiguration.magazineSize;
            _reloading = false;
        }

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}