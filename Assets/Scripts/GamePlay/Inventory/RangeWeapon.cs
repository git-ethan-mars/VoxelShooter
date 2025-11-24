using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Core;
using GamePlay.MapFeatures;
using Mirror;
using Networking.Audio;
using Networking.Core;
using R3;
using Services;
using UnityEngine;
using AudioType = Data.AudioType;
using Random = UnityEngine.Random;
namespace GamePlay
{
	public abstract class RangeWeapon : InventoryItem
	{
		private const float SpreadThreshold = 1e-3f;

		[SerializeField] private AudioType shootSound;
		[SerializeField] private AudioType reloadSound;
		[SerializeField] private ParticleSystem shootingParticles;

		protected IInputService InputService;
		protected NetworkAudioPlayer AudioPlayer;
		protected CameraProvider CameraProvider;

		private CancellationTokenSource _onChangeSlot;
		private bool _isReloading;
		private bool _isReady = true;
		private float _recoilModifier;
		private readonly SyncReactiveProperty<int> _totalBullets = new SyncReactiveProperty<int>();
		private readonly SyncReactiveProperty<int> _bulletsInMagazine = new SyncReactiveProperty<int>();
		private readonly ReactiveProperty<bool> _isZoomed = new ReactiveProperty<bool>(false);
		public ReactiveProperty<int> TotalBullets => _totalBullets;
		public ReactiveProperty<int> BulletsInMagazine => _bulletsInMagazine;
		public Observable<bool> IsZoomed => _isZoomed;
		public new RangeWeaponConfigure Configure => base.Configure as RangeWeaponConfigure;

		public override void OnStartServer()
		{
			base.OnStartServer();

			_totalBullets.Value = Configure.TotalBullets;
			_bulletsInMagazine.Value = Configure.MagazineSize;
			
			ResetRecoil(destroyCancellationToken).Forget();
		}
		
		protected void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			if (InputService.IsFirstActionButtonDown())
			{
				Shoot(CameraProvider.CentredRay);
			}
			else if (Configure.IsAutomatic)
			{
				if (InputService.IsFirstActionButtonHold())
				{
					Shoot(CameraProvider.CentredRay);

					if (_bulletsInMagazine.Value <= 0)
					{
						shootingParticles.Stop();
					}
				}
				else
				{
					shootingParticles.Stop();
				}
			}
			if (InputService.IsSecondActionButtonDown())
			{
				_isZoomed.Value = !_isZoomed.Value;
			}
			if (InputService.IsReloadingButtonDown() && CanReload())
			{
				Reload();
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
			if (!_isReady)
			{
				ResetShoot();
			}
		}

		public override void Deselect()
		{
			base.Deselect();
			_onChangeSlot?.Cancel();
			_onChangeSlot?.Dispose();

			_isZoomed.Value = false;
		}

		[Command]
		private void Shoot(Ray ray)
		{
			if (!CanShoot())
			{
				return;
			}

			for (var i = 0; i < Configure.BulletsPerTap; i++)
			{
				Vector3 spread = GetRandomSpreadDirection();
				ray = new Ray(ray.origin, ray.direction + spread);
				ScanHit(ray);
				_recoilModifier += Configure.StepRecoil;
			}

			_isReady = false;
			_bulletsInMagazine.Value -= 1;
			AudioPlayer.SendAudio(shootSound, netIdentity, false);
			shootingParticles.Play();
			ResetShoot();
		}

		[Command]
		private async void Reload()
		{
			_isReloading = true;
			AudioPlayer.SendAudio(reloadSound, netIdentity, false);

			bool isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(Configure.ReloadTime), cancellationToken: _onChangeSlot.Token)
					.SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}

			if (_totalBullets.Value + _bulletsInMagazine.Value - Configure.MagazineSize <= 0)
			{
				_bulletsInMagazine.Value += _totalBullets.Value;
				_totalBullets.Value = 0;
			}
			else
			{
				_totalBullets.Value -= Configure.MagazineSize - _bulletsInMagazine.Value;
				_bulletsInMagazine.Value = Configure.MagazineSize;
			}

			_isReloading = false;
		}

		private async UniTask ResetRecoil(CancellationToken token)
		{
			while (!destroyCancellationToken.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(Configure.ResetTimeRecoil), cancellationToken: token);
				_recoilModifier = Mathf.Max(_recoilModifier - Configure.StepRecoil * Configure.BulletsPerTap, 0);
			}
		}

		[Server]
		private async void ResetShoot()
		{
			bool isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(Configure.TimeBetweenShooting),
						cancellationToken: _onChangeSlot.Token)
					.SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}

			_isReady = true;
		}

		[Server]
		private void ScanHit(Ray ray)
		{
			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, Configure.Range, LayerMasks.AttackMask);
			if (!raycastResult)
			{
				return;
			}

			var damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();
			damageVisitor?.Visit(this, rayHit);
		}

		private Vector3 GetRandomSpreadDirection()
		{
			float x = GetSpreadByAxis();
			float y = GetSpreadByAxis();
			float z = GetSpreadByAxis();
			return new Vector3(x, y, z);
		}

		private float GetSpreadByAxis()
		{
			return Math.Abs(_recoilModifier) < SpreadThreshold ? 0
				: Random.Range(-Configure.BaseRecoil, Configure.BaseRecoil) * (_recoilModifier + 1);
		}

		private bool CanShoot()
		{
			return _isReady && !_isReloading && _bulletsInMagazine.Value > 0;
		}

		private bool CanReload()
		{
			return _bulletsInMagazine.Value < Configure.MagazineSize && !_isReloading && _totalBullets.Value > 0;
		}
	}
}