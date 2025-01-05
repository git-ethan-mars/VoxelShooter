using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
    public class RocketLauncher : InventoryItem
    {
        public override Sprite InventoryIcon => Data.InventoryIcon;
        public RocketLauncherData Data { get; private set; }
        private IEntityFactory _entityFactory;
        private RayCaster _rayCaster;
        private IInputService _inputService;
        private MapProvider _mapProvider;

        public void Construct(IInputService inputService, IEntityFactory entityFactory, RocketLauncherData data,
            RayCaster rayCaster, MapProvider mapProvider)
        {
            _inputService = inputService;
            _entityFactory = entityFactory;
            Data = data;
            _rayCaster = rayCaster;
            _mapProvider = mapProvider;
        }

        private async void Update()
        {
            if (_inputService.IsFirstActionButtonDown())
            {
                Shoot();
            }

            if (_inputService.IsReloadingButtonDown())
            {
                await ReloadAsync();
            }
        }

        private void Shoot()
        {
            if (!CanShoot())
            {
                return;
            }

            var ray = _rayCaster.CentredRay;
            var rocketPosition = ray.origin + ray.direction * 3;
            var rocketRotation = Quaternion.LookRotation(ray.direction);
            var rocket = _entityFactory.CreateRocket(rocketPosition, rocketRotation, Data, _mapProvider);
            rocket.GetComponent<Rigidbody>().linearVelocity = rocket.transform.forward * Data.Speed;
            Data.ChargedRockets -= 1;
        }

        private async UniTask ReloadAsync()
        {
            if (!CanReload())
            {
                return;
            }

            Data.IsReloading = true;
            await UniTask.WaitForSeconds(Data.ReloadTime);
            Data.IsReloading = false;
            Data.CarriedRockets -= Data.RechargeableRocketsCount;
            Data.ChargedRockets += Data.RechargeableRocketsCount;
        }

        private bool CanReload()
        {
            return Data.CarriedRockets > 0 && !Data.IsReloading &&
                   Data.ChargedRockets < Data.ChargedRocketsCapacity;
        }

        private bool CanShoot()
        {
            return !Data.IsReloading && Data.ChargedRockets > 0;
        }
    }
}