using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Audio;
using GamePlay.Core;
using GamePlay.MapFeatures;
using Mirror;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public abstract class MeleeWeapon : InventoryItem
	{
		[SerializeField] protected Material wireframeMaterial;
		[SerializeField] protected Mesh wireframeCube;
		[SerializeField] private AudioData digSound;
		[SerializeField] protected AudioData hitSound;
		
		private CancellationTokenSource _onChangeSlot;

		protected IInputService InputService { get; set; }
		protected CameraService CameraService { get; set; }
		protected AudioPlayer AudioPlayer { get; set; }
		public new MeleeWeaponConfigure Configure => base.Configure as MeleeWeaponConfigure;

		private bool _isReady = true;

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}
			
			if (InputService.IsFirstActionButtonDown())
			{
				Hit(CameraService.CentredRay, false);
			}
			if (Configure.HasStrongHit && InputService.IsSecondActionButtonDown())
			{
				Hit(CameraService.CentredRay, true);
			}

			if (CameraService.GetBuildRayCastHit(out RaycastHit hit, Configure.Range))
			{
				var voxelPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2) + Map.WorldOffset;
				Graphics.DrawMesh(wireframeCube, Matrix4x4.TRS(voxelPosition, Quaternion.identity, Vector3.one * 1.001f), 
					wireframeMaterial, 0);
			}
		}

		internal override void Select()
		{
			base.Select();
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			ResetHit(_onChangeSlot.Token).Forget();
		}

		internal override void Deselect()
		{
			base.Deselect();
			_onChangeSlot.Cancel();
			_onChangeSlot.Dispose();
		}

		[Command]
		private void Hit(Ray ray, bool isStrongHit)
		{
			if (!_isReady)
			{
				return;
			}

			_isReady = false;
			ScanHit(ray, isStrongHit);
		}

		private void ScanHit(Ray ray, bool isStrongHit)
		{
			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, Configure.Range, LayerMasks.AttackMask);
			if (!raycastResult)
			{
				return;
			}

			var damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();

			if (rayHit.collider.GetComponent<IDamageaeble>() != null)
			{
				AudioPlayer.Play(hitSound, rayHit.point);
			}
			else
			{
				AudioPlayer.Play(digSound, rayHit.point);
			}
			
			damageVisitor?.Visit(this, isStrongHit, rayHit);
		}

		private async UniTask ResetHit(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(Configure.TimeBetweenHit), cancellationToken: token);
				_isReady = true;
			}
		}
	}
}