using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using AudioType = Data.AudioType;
namespace GamePlay
{
	public class DrillLauncher : InventoryItem
	{
		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private CameraProvider _cameraProvider;
		private NetworkAudioSender _audioSender;

		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		private bool _isReloading;
		private CancellationTokenSource _onChangeSlot;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory, CameraProvider cameraProvider,
			IStaticDataService staticData, NetworkAudioSender audioSender)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraProvider = cameraProvider;
			_audioSender = audioSender;
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
				Shoot(_cameraProvider.CentredRay);
			}
		}

		public override void Select()
		{
			base.Select();
			
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

			if (_isReloading)
			{
				Reload();
			}
		}

		public override void Deselect()
		{
			base.Deselect();
			
			_onChangeSlot?.Cancel();
			_onChangeSlot?.Dispose();
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
			Drill drill = _entityFactory.CreateDrill(drillPosition, drillRotation, connectionToClient);
			drill.Launch();
			_amount.Value -= 1;

			if (_amount.Value > 0)
			{
				Reload();
			}
		}

		[Server]
		private async void Reload()
		{
			_audioSender.SendAudio(AudioType.DrillLauncherReload, netIdentity, false);
			
			_isReloading = true;
			bool isCanceled = await UniTask.WaitForSeconds(Configure.ReloadTime, cancellationToken: _onChangeSlot.Token).SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}
			
			_isReloading = false;
		}

		private bool CanShoot()
		{
			return !_isReloading && _amount.Value > 0;
		}
	}
}