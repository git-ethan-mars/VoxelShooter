using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Inventory.RangeWeapon
{
	public class RangeWeapon : MonoBehaviour, IInventoryItem
	{
		public RangeWeaponData Data { get; private set; }
		private CancellationToken _onDestroyToken;
		private CancellationTokenSource _onChangeSlot;

		public void Construct(RangeWeaponData data)
		{
			Data = data;
			_onDestroyToken = this.GetCancellationTokenOnDestroy();
		}

		private async void Start()
		{
			await ResetRecoil(_onDestroyToken);
		}

		public async void Enable()
		{
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
			if (Data.IsReloading)
			{
				await ReloadInternal(_onChangeSlot.Token);
			}
		}

		public void Disable()
		{
			_onChangeSlot.Cancel();
			_onChangeSlot.Dispose();
		}

		private void ScanHit(Ray ray)
		{
			var raycastResult = Physics.Raycast(ray, out var rayHit, Data.Range, Constants.attackMask);
			if (!raycastResult)
			{
				return;
			}

			var damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();
			damageVisitor?.Visit(Data, rayHit);
		}

		private async Task Shoot(Ray ray, bool requestIsButtonHolding)
		{
			if (!CanShoot() || requestIsButtonHolding != Data.IsAutomatic)
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
			
			await ResetShoot(_onChangeSlot.Token);
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
				await UniTask.Delay(TimeSpan.FromSeconds(Data.ReloadTime), cancellationToken: token).SuppressCancellationThrow();

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

		private async UniTask ResetShoot(CancellationToken token)
		{
			var isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(Data.TimeBetweenShooting), cancellationToken: token)
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
			return Math.Abs(Data.RecoilModifier) < Constants.Epsilon
				? 0
				: UnityEngine.Random.Range(-Data.BaseRecoil, Data.BaseRecoil) *
				  (Data.RecoilModifier + 1);
		}

		private bool CanShoot()
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