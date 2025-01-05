using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Data;
using GamePlay.MapFeatures;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
	public abstract class MeleeWeapon : InventoryItem
	{
		public override Sprite InventoryIcon => _data.InventoryIcon;
		
		protected IInputService InputService;
		protected RayCaster RayCaster;
		private MeleeWeaponData _data;
		private CancellationToken _onDestroyToken;
		private CancellationTokenSource _onChangeSlot;


		public void Construct(IInputService inputService, MeleeWeaponData data, RayCaster rayCaster)
		{
			InputService = inputService;
			_data = data;
			RayCaster = rayCaster;
			_onDestroyToken = this.GetCancellationTokenOnDestroy();
		}

		internal override void Select()
		{
			base.Select();
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(_onDestroyToken);
		}

		internal override void Deselect()
		{
			base.Deselect();
			_onChangeSlot.Cancel();
			_onChangeSlot.Dispose();
		}

		protected async void Hit(Ray ray, bool isStrongHit)
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
			var raycastResult = Physics.Raycast(ray, out var rayHit, _data.Range, LayerMasks.AttackMask);
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