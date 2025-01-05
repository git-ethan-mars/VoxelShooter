using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.Entities;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
	public class DrillLauncher : InventoryItem
	{
		public override Sprite InventoryIcon => Data.InventoryIcon;

		public DrillLauncherData Data { get; private set; }
		private RayCaster _rayCaster;
		private IInputService _inputService;
		private IEntityFactory _entityFactory;


		public void Construct(IInputService inputService,IEntityFactory entityFactory, DrillLauncherData data, RayCaster rayCaster)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			Data = data;
			_rayCaster = rayCaster;
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
            var drillPosition = ray.origin + ray.direction * 3;
            var drillRotation = Quaternion.LookRotation(ray.direction);
            
            var drill = _entityFactory.CreateDrill(drillPosition, drillRotation, Data);
            drill.GetComponent<Rigidbody>().linearVelocity = ray.direction * Data.Speed;
            Data.ChargedDrills -= 1;
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
            Data.Amount -= 1;
            Data.ChargedDrills += 1;
        }

		private bool CanReload()
        {
            return Data.Amount > 0 && !Data.IsReloading && Data.ChargedDrills < 1;
        }

		private bool CanShoot()
        {
            return !Data.IsReloading && Data.ChargedDrills > 0;
        }
	}
}