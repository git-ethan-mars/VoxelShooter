using System;
using System.Collections;
using Core;
using Infrastructure;
using Infrastructure.Factory;
using TMPro;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class GunSystem : IInventoryItemView, ILeftMouseButtonDownHandler, ILeftMouseButtonHoldHandler, IUpdated
    {
        public Sprite Icon { get; }
        public readonly string guid;
        private readonly PrimaryWeapon _weaponConfiguration;
        private readonly TextMeshProUGUI _ammoInfo;
        private readonly Transform _attackPoint;
        public bool _readyToShoot;
        public float stepRecoil;
        private bool _reloading;
        public int bulletsInMagazine;
        public int magazineSize;
        public int totalBullets;
        public int _bulletsPerShot;
        private readonly Camera _fpsCam;
        public float _recoilModifier;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IGameFactory _gameFactory;
        private readonly GameObject _weaponModel;
        private readonly RaycastSynchronization _raycastSyncronization;
        private readonly StatSynchronization _statSyncronization;

        public GunSystem(IGameFactory gameFactory, ICoroutineRunner coroutineRunner, string weaponPath, Camera camera,
            Transform itemPosition, RaycastSynchronization raycastSynchronization, StatSynchronization statSynchronization)
        {
            _gameFactory = gameFactory;
            _coroutineRunner = coroutineRunner;
            _weaponConfiguration = Resources.Load<PrimaryWeapon>(weaponPath);
            _weaponModel = Object.Instantiate(_weaponConfiguration.prefab,
                itemPosition);
            _weaponModel.SetActive(false);
            Icon = _weaponConfiguration.icon;
            _attackPoint = _weaponModel.GetComponentInChildren<Transform>();
            _fpsCam = camera;
            _ammoInfo = GameObject.Find("Canvas/GamePlay/AmmoAmount").GetComponent<TextMeshProUGUI>();
            bulletsInMagazine = _weaponConfiguration.magazineSize;
            _readyToShoot = true;
            _raycastSyncronization = raycastSynchronization;
            magazineSize = _weaponConfiguration.magazineSize;
            guid = Convert.ToString(Guid.NewGuid());
            stepRecoil = _weaponConfiguration.stepRecoil;
            _statSyncronization = statSynchronization;
        }

        public void Select()
        {
            _ammoInfo.gameObject.SetActive(true);
            _weaponModel.SetActive(true);
        }


        public void Unselect()
        {
            _ammoInfo.gameObject.SetActive(false);
            _weaponModel.SetActive(false);
        }

        public void InnerUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R) && bulletsInMagazine < _weaponConfiguration.magazineSize && !_reloading)
                Reload();
            _ammoInfo.SetText($"{bulletsInMagazine} / {_weaponConfiguration.magazineSize}");
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
            if (_readyToShoot && !_reloading && bulletsInMagazine > 0)
            {
                _bulletsPerShot = _weaponConfiguration.bulletsPerTap;
                Shoot();
            }

            _ammoInfo.SetText($"{bulletsInMagazine} / {_weaponConfiguration.magazineSize}");
        }

        private void Shoot()
        {
            var x = Math.Abs(_recoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * _recoilModifier;
            var y = Math.Abs(_recoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * _recoilModifier;

            var direction = new Vector3(0.5f, 0.5f);
            if (_weaponConfiguration.isAutomatic)
                direction += new Vector3(x, y);

            var ray = _fpsCam.ViewportPointToRay(direction);
            _raycastSyncronization.ApplyRaycast(ray.origin, ray.direction, _weaponConfiguration.range,
                _weaponConfiguration.damage);

            _gameFactory.CreateMuzzleFlash(_attackPoint);
            
            _statSyncronization.OnShoot(guid);
            
        }

        public void StartShootCoroutines()
        {
            
            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetShoot, _weaponConfiguration.timeBetweenShooting));
            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetRecoil, _weaponConfiguration.resetTimeRecoil));
            
            if (_bulletsPerShot > 0 && bulletsInMagazine > 0)
                _coroutineRunner.StartCoroutine(WaitForSeconds(Shoot, _weaponConfiguration.timeBetweenShots));
        }

        private void ResetRecoil()
        {
            _recoilModifier -= _weaponConfiguration.stepRecoil;
        }

        private void ResetShoot()
        {
            _readyToShoot = true;
        }

        private void Reload()
        {
            _reloading = true;
            _statSyncronization.OnReload(guid);
            StartReloadCoroutine();
        }

        public void StartReloadCoroutine()
        {
            _coroutineRunner.StartCoroutine(WaitForSeconds(ReloadFinished, _weaponConfiguration.reloadTime));
        }

        private void ReloadFinished()
        {
            bulletsInMagazine = _weaponConfiguration.magazineSize;
            _reloading = false;
        }

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}