using System;
using System.Threading;
using Common;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using GamePlay.Destruction;
using UnityEngine;

namespace Inventory.MeleeWeapon
{
	public class MeleeWeapon : MonoBehaviour, IInventoryItem
	{
		private MeleeWeaponData _data;
		private CancellationToken _onDestroyToken;
		private CancellationTokenSource _onChangeSlot;

		public void Construct(MeleeWeaponData data)
		{
			_data = data;
			_onDestroyToken = this.GetCancellationTokenOnDestroy();
		}

		public void Enable()
		{
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
		}

		public void Disable()
		{
			_onChangeSlot.Cancel();
			_onChangeSlot.Dispose();
		}
        
		public async void Hit(Ray ray, bool isStrongHit)
		{
			if (!CanHit(_data))
			{
				return;
			}

			_data.IsReady = false;
			ScanHit(ray, isStrongHit);
			await ResetHit(_onChangeSlot.Token);
		}
		
		private void ScanHit(Ray ray, bool isStrongHit)
		{
			var raycastResult = Physics.Raycast(ray, out var rayHit, _data.Range, Constants.attackMask);
			if (!raycastResult)
			{
				return;
			}

			var damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();
			damageVisitor?.Visit(_data, isStrongHit, rayHit);
		}

		private async UniTask ResetHit(CancellationToken token)
		{
			var isCanceled =
				await UniTask.Delay(TimeSpan.FromSeconds(_data.TimeBetweenHit), cancellationToken: token)
					.SuppressCancellationThrow();

			if (isCanceled)
			{
				return;
			}

			_data.IsReady = true;
		}

		private bool CanHit(MeleeWeaponData meleeWeapon)
		{
			return meleeWeapon.IsReady;
		}	
	}
}