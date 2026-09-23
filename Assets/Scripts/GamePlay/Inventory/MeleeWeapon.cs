using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
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
		[SerializeField] protected AudioType hitSound;
		[SerializeField] private AudioType digSound;

		private CancellationTokenSource _onChangeSlot;

		private bool _isReady = true;
		public new MeleeWeaponConfigure Configure => base.Configure as MeleeWeaponConfigure;

		protected IInputService InputService { get; set; }
		protected CameraProvider CameraProvider { get; set; }
		protected NetworkAudioSender AudioSender { get; set; }

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			if (InputService.IsFirstActionButtonDown())
			{
				Hit(CameraProvider.CentredRay, false);
			}

			if (Configure.HasStrongHit && InputService.IsSecondActionButtonDown())
			{
				Hit(CameraProvider.CentredRay, true);
			}

			if (CameraProvider.GetBuildRayCastHit(out RaycastHit hit, Configure.Range))
			{
				var voxelPosition = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);

				if (voxelPosition.y == 0)
				{
					return;
				}

				Graphics.DrawMesh(wireframeCube, Matrix4x4.TRS(voxelPosition + Map.WorldOffset, Quaternion.identity, Vector3.one * 1.001f),
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

		[Server]
		private void ScanHit(Ray ray, bool isStrongHit)
		{
			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, Configure.Range, LayerMasks.AttackMask);

			if (!raycastResult)
			{
				return;
			}

			MapDestruction mapDestruction = rayHit.collider.GetComponentInParent<MapDestruction>();

			if (mapDestruction != null)
			{
				mapDestruction.Visit(this, isStrongHit, rayHit);
				AudioSender.SendAudio(digSound, rayHit.point);
				return;
			}

			IDamageVisitor damageVisitor = rayHit.collider.GetComponentInParent<IDamageVisitor>();

			if (damageVisitor != null)
			{
				AudioSender.SendAudio(hitSound, rayHit.point);
				damageVisitor.Visit(this, rayHit);
			}
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
