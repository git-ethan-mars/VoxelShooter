using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Core;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class RocketLauncher : InventoryItem
	{
		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private CameraService _cameraService;

		private bool _isReloading;
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		private readonly SyncReactiveProperty<int> _chargedRockets = new SyncReactiveProperty<int>();
		public ReactiveProperty<int> Amount => _amount;
		public ReactiveProperty<int> ChargedRockets => _chargedRockets;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory, CameraService cameraService,
			IStaticDataService staticData)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraService = cameraService;
		}

		public override ItemType Type => ItemType.RocketLauncher;
		private new RocketLauncherConfigure Configure => base.Configure as RocketLauncherConfigure;

		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
			_chargedRockets.Value = Configure.ChargedRocketsCapacity;
		}

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			if (_inputService.IsFirstActionButtonDown())
			{
				Shoot(_cameraService.CentredRay);
			}

			if (_inputService.IsReloadingButtonDown())
			{
				ReloadAsync();
			}
		}

		[Command]
		private void Shoot(Ray ray)
		{
			if (!CanShoot())
			{
				return;
			}

			Vector3 rocketPosition = ray.origin + ray.direction * 3;
			Quaternion rocketRotation = Quaternion.LookRotation(ray.direction);
			Rocket rocket = _entityFactory.CreateRocket(rocketPosition, rocketRotation);
			rocket.Launch();
			_chargedRockets.Value -= 1;
		}

		[Command]
		private async void ReloadAsync()
		{
			if (!CanReload())
			{
				return;
			}

			_isReloading = true;
			await UniTask.WaitForSeconds(Configure.ReloadTime, cancellationToken: destroyCancellationToken);
			_isReloading = false;
			_amount.Value -= 1;
			_chargedRockets.Value += 1;
		}

		private bool CanReload()
		{
			return !_isReloading && _chargedRockets.Value < Configure.ChargedRocketsCapacity && _amount.Value > 0;
		}

		private bool CanShoot()
		{
			return !_isReloading && _chargedRockets.Value > 0;
		}
	}
}