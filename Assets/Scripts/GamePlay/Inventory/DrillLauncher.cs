using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Audio;
using GamePlay.Core;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay
{
	public class DrillLauncher : InventoryItem
	{
		[SerializeField] private AudioData reloadSound;
		
		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private CameraService _cameraService;
		private AudioPlayer _audioPlayer;

		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		private bool _isReloading;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory, CameraService cameraService,
			IStaticDataService staticData, AudioPlayer audioPlayer)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraService = cameraService;
			_audioPlayer = audioPlayer;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
		}

		public override ItemType Type => ItemType.DrillLauncher;
		private new DrillLauncherConfigure Configure => base.Configure as DrillLauncherConfigure;
		public ReactiveProperty<int> Amount => _amount;
		
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
				Reload();
			}
		}

		[Command]
		private void Shoot(Ray ray)
		{
			if (!CanShoot())
			{
				return;
			}

			Vector3 drillPosition = ray.origin + ray.direction * 3;
			Quaternion drillRotation = Quaternion.LookRotation(ray.direction);
			Drill drill = _entityFactory.CreateDrill(drillPosition, drillRotation);
			drill.Launch();
			_amount.Value -= 1;
		}

		[Command]
		private async void Reload()
		{
			if (!CanReload())
			{
				return;
			}
			
			_audioPlayer.Play(reloadSound, transform, false);
			
			_isReloading = true;
			await UniTask.WaitForSeconds(Configure.ReloadTime, cancellationToken: destroyCancellationToken);
			_isReloading = false;
		}

		private bool CanReload()
		{
			return _amount.Value > 0 && !_isReloading;
		}

		private bool CanShoot()
		{
			return !_isReloading && _amount.Value > 0;
		}
	}
}