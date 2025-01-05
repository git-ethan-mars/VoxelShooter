using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Audio;
using GamePlay.Data;
using GamePlay.MapFeatures;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
	public abstract class RangeWeapon : InventoryItem
	{
		private const float SpreadThreshold = 1e-3f;
		
		[SerializeField] 
		private ParticleSystem shootingParticles;

		public override Sprite InventoryIcon => Data.InventoryIcon;
		public RangeWeaponData Data { get; private set; }
		private IInputService _inputService;
		private RayCaster _rayCaster;

		private CancellationTokenSource _onChangeSlot;
		private CancellationToken _onDestroyToken;


		public void Construct(IInputService inputService, RangeWeaponData data, RayCaster rayCaster)
		{
			_inputService = inputService;
			_rayCaster = rayCaster;
			Data = data;
			_onDestroyToken = this.GetCancellationTokenOnDestroy();
		}

		private async void Start()
		{
			await ResetRecoil(_onDestroyToken);
		}

		internal override async void Select()
		{
			base.Select();
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
			if (Data.IsReloading)
			{
				await ReloadInternal(_onChangeSlot.Token);
			}
		}

		internal override void Deselect()
		{
			base.Deselect();
			_onChangeSlot.Cancel();
			_onChangeSlot.Dispose();
		}

		protected void Update()
		{
			if (_inputService.IsFirstActionButtonDown())
			{
				Shoot(_rayCaster.CentredRay).Forget();
			}
			else if (Data.IsAutomatic)
			{
				if (_inputService.IsFirstActionButtonHold())
				{
					Shoot(_rayCaster.CentredRay).Forget();
					if (Data.BulletsInMagazine <= 0)
					{
						shootingParticles.Stop();
					}
				}
				else
				{
					shootingParticles.Stop();
				}
			}
		}

		private void ScanHit(Ray ray)
		{
			var raycastResult = Physics.Raycast(ray, out var rayHit, Data.Range, LayerMasks.AttackMask);
			if (!raycastResult)
			{
				return;
			}

			var damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();
			damageVisitor?.Visit(Data, rayHit);
		}

		private async UniTaskVoid Shoot(Ray ray)
		{
			if (!CanShoot())
			{
				return;
			}

			for (var i = 0; i < Data.BulletsPerTap; i++)
			{
				var spread = GetRandomSpreadDirection();
				ray = new Ray(ray.origin, ray.direction + spread);
				ScanHit(ray);
				Data.RecoilModifier += Data.StepRecoil;
			}

			Data.IsReady = false;
			Data.BulletsInMagazine -= 1;
			AudioPlayer.Play(transform.position, Data.ShootingSound);
			shootingParticles.Play();
			await ResetShoot();
		}

		public async void Reload(CancellationToken reloadToken)
		{
			if (!CanReload())
			{
				return;
			}

			Data.IsReloading = true;
			await ReloadInternal(reloadToken);
		}

		private async UniTask ReloadInternal(CancellationToken token)
		{
			var isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(Data.ReloadTime), cancellationToken: token)
					.SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}

			if (Data.TotalBullets + Data.BulletsInMagazine - Data.MagazineSize <= 0)
			{
				Data.BulletsInMagazine += Data.TotalBullets;
				Data.TotalBullets = 0;
			}
			else
			{
				Data.TotalBullets -= Data.MagazineSize - Data.BulletsInMagazine;
				Data.BulletsInMagazine = Data.MagazineSize;
			}

			Data.IsReloading = false;
		}

		private async UniTask ResetRecoil(CancellationToken token)
		{
			while (!_onDestroyToken.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(Data.ResetTimeRecoil), cancellationToken: token)
					.SuppressCancellationThrow();
				Data.RecoilModifier -= Data.StepRecoil * Data.BulletsPerTap; // TODO : TEST RECOIL
			}
		}

		private async UniTask ResetShoot()
		{
			var isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(Data.TimeBetweenShooting),
						cancellationToken: _onChangeSlot.Token)
					.SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}

			Data.IsReady = true;
		}

		private Vector3 GetRandomSpreadDirection()
		{
			var x = GetSpreadByAxis();
			var y = GetSpreadByAxis();
			var z = GetSpreadByAxis();
			return new Vector3(x, y, z);
		}


		private float GetSpreadByAxis()
		{
			return Math.Abs(Data.RecoilModifier) < SpreadThreshold
				? 0
				: UnityEngine.Random.Range(-Data.BaseRecoil, Data.BaseRecoil) *
				  (Data.RecoilModifier + 1);
		}

		protected bool CanShoot()
		{
			return Data.IsReady && !Data.IsReloading && Data.BulletsInMagazine > 0;
		}

		private bool CanReload()
		{
			return Data.BulletsInMagazine < Data.MagazineSize &&
			       !Data.IsReloading && Data.TotalBullets > 0;
		}
	}
}