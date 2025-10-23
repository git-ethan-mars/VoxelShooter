using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Core;
using GamePlay.MapFeatures;
using Mirror;
using Networking.Audio;
using Services;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;
namespace GamePlay
{
	public abstract class MeleeWeapon : InventoryItem
	{
		[SerializeField] protected Material wireframeMaterial;
		[SerializeField] protected Mesh wireframeCube;
		[SerializeField] private AudioType digSound;
		[SerializeField] protected AudioType hitSound;
		
		private CancellationTokenSource _onChangeSlot;

		protected IInputService InputService { get; set; }
		protected CameraService CameraService { get; set; }
		protected NetworkAudioPlayer AudioPlayer { get; set; }
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

		
		public override void Select()
		{
			base.Select();
			_onChangeSlot = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			ResetHit(_onChangeSlot.Token);
		}

		public override void Deselect()
		{
			base.Deselect();
			_onChangeSlot?.Cancel();
			_onChangeSlot?.Dispose();
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
				AudioPlayer.SendAudio(hitSound, rayHit.point);
			}
			else
			{
				AudioPlayer.SendAudio(digSound, rayHit.point);
			}
			
			damageVisitor?.Visit(this, isStrongHit, rayHit);
		}

		[Server]
		private async void ResetHit(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(Configure.TimeBetweenHit), cancellationToken: token).SuppressCancellationThrow();
				_isReady = true;
			}
		}
	}
}