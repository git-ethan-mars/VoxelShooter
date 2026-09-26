using Cysharp.Threading.Tasks;
using Data;
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
		private CameraProvider _cameraProvider;

		private bool _isReloading;
		private bool _isAutoReloadEnabled;
		private bool _isAutoReloadRequested;
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		private readonly SyncReactiveProperty<int> _chargedRockets = new SyncReactiveProperty<int>();
		public ReactiveProperty<int> Amount => _amount;
		public ReactiveProperty<int> ChargedRockets => _chargedRockets;

		public override ItemType Type => ItemType.RocketLauncher;
		private new RocketLauncherConfigure Configure => base.Configure as RocketLauncherConfigure;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory, CameraProvider cameraProvider,
			IStaticDataService staticData, IStorageService storageService)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraProvider = cameraProvider;

			_isAutoReloadEnabled = storageService.Load<GameplaySettingsData>(IStorageService.GameplaySettingsKey).AutoReload;
			storageService.Subscribe<GameplaySettingsData>(settings => _isAutoReloadEnabled = settings.AutoReload)
				.AddTo(this);
		}

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
				Shoot(_cameraProvider.CentredRay);
			}

			if (_inputService.IsReloadingButtonDown())
			{
				ReloadAsync();
			}

			TryAutoReload();
		}

		// Sends a single reload request each time the charged rockets run out.
		private void TryAutoReload()
		{
			if (_chargedRockets.Value > 0)
			{
				_isAutoReloadRequested = false;
				return;
			}

			if (_isAutoReloadEnabled && !_isAutoReloadRequested && CanReload())
			{
				_isAutoReloadRequested = true;
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
			var rocketRotation = Quaternion.LookRotation(ray.direction);
			Rocket rocket = _entityFactory.CreateRocket(rocketPosition, rocketRotation, connectionToClient);
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
