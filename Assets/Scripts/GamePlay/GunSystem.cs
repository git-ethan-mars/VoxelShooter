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
        private readonly PrimaryWeapon _weaponConfiguration;
        private readonly TextMeshProUGUI _ammoInfo;
        private readonly Transform _attackPoint;
        private bool _readyToShoot;
        private bool _reloading;
        private int _bulletsLeft;
        private int _bulletsShot;
        private readonly Camera _fpsCam;
        private float _recoilModifier;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IGameFactory _gameFactory;
        private readonly GameObject _weaponModel;
        private readonly MapSynchronization _mapSyncronization;

        public GunSystem(IGameFactory gameFactory, ICoroutineRunner coroutineRunner, string weaponPath, Camera camera,
            Transform itemPosition, MapSynchronization mapSynchronization)
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
            _bulletsLeft = _weaponConfiguration.magazineSize;
            _readyToShoot = true;
            _mapSyncronization = mapSynchronization;
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
            if (Input.GetKeyDown(KeyCode.R) && _bulletsLeft < _weaponConfiguration.magazineSize && !_reloading)
                Reload();
            _ammoInfo.SetText($"{_bulletsLeft} / {_weaponConfiguration.magazineSize}");
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
            if (_readyToShoot && !_reloading && _bulletsLeft > 0)
            {
                _bulletsShot = _weaponConfiguration.bulletsPerTap;
                Shoot();
            }

            _ammoInfo.SetText($"{_bulletsLeft} / {_weaponConfiguration.magazineSize}");
        }

        private void Shoot()
        {
            var x = Math.Abs(_recoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * _recoilModifier;
            var y = Math.Abs(_recoilModifier - 1) < 0.00001
                ? 0
                : Random.Range(-_weaponConfiguration.baseRecoil, _weaponConfiguration.baseRecoil) * _recoilModifier;
            _recoilModifier += _weaponConfiguration.stepRecoil;

            _readyToShoot = false;

            var direction = new Vector3(0.5f, 0.5f);
            if (_weaponConfiguration.isAutomatic)
                direction += new Vector3(x, y);

            var ray = _fpsCam.ViewportPointToRay(direction);
            _mapSyncronization.ApplyRaycast(ray.origin, ray.direction, _weaponConfiguration.range,
                _weaponConfiguration.damage);


            _gameFactory.CreateMuzzleFlash(_attackPoint);
            _bulletsLeft--;
            _bulletsShot--;

            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetShoot, _weaponConfiguration.timeBetweenShooting));
            _coroutineRunner.StartCoroutine(WaitForSeconds(ResetRecoil, _weaponConfiguration.resetTimeRecoil));

            if (_bulletsShot > 0 && _bulletsLeft > 0)
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
            _coroutineRunner.StartCoroutine(WaitForSeconds(ReloadFinished, _weaponConfiguration.reloadTime));
        }

        private void ReloadFinished()
        {
            _bulletsLeft = _weaponConfiguration.magazineSize;
            _reloading = false;
        }

        private static IEnumerator WaitForSeconds(Action action, float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            action();
        }
    }
}