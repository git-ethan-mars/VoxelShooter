using System;
using Mirror;
using Networking;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;

namespace GamePlay
{
	[SelectionBase]
	public abstract class LootBox : Entity
	{
		private const string LootBoxContainer = "LootBoxContainer";

		private static Transform _lootBoxRoot;

		[SerializeField] private Sprite miniMapImage;
		[SerializeField] private new Collider collider;
		[SerializeField] private GameObject parachute;
		[SerializeField] private GameObject platformPrefab;
		[SerializeField] private Bounds localBounds;

		private readonly Subject<Unit> _pickedUp = new Subject<Unit>();
		
		private MapProvider _mapProvider;
		private IAssetProvider _assets;
		private NetworkAudioSender _audioSender;

		private GameObject _platform;

		[Inject]
		private void Construct(EntityContainer entityContainer, MapProvider mapProvider, IAssetProvider assets, NetworkAudioSender audioSender)
		{
			EntityContainer = entityContainer;
			_mapProvider = mapProvider;
			_assets = assets;
			_audioSender = audioSender;
		}

		private void Start()
		{
			if (_lootBoxRoot == null)
			{
				_lootBoxRoot = new GameObject(LootBoxContainer).transform;
			}

			transform.SetParent(_lootBoxRoot.transform);

			int platformPositionX = Mathf.FloorToInt(transform.position.x);
			int platformPositionZ = Mathf.FloorToInt(transform.position.z);
			int platformPositionY = GetTopVoxelHeight((ushort)platformPositionX, (ushort)platformPositionZ);
			Vector3 platformPosition = new Vector3(platformPositionX, platformPositionY, platformPositionZ) + Map.WorldOffset + Vector3.up * 0.5f;
			_platform = _assets.Instantiate(platformPrefab, _lootBoxRoot);
			_platform.transform.position = platformPosition;
		}

		private void OnDestroy()
		{
			if (_platform != null)
			{
				Destroy(_platform);
			}
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_mapProvider.Map.CurrentValue.MapUpdated
				.Subscribe(_ => ValidatePosition())
				.AddTo(this);
		}

		private void OnCollisionEnter(Collision other)
		{
			var character = other.gameObject.GetComponent<Character>();

			if (character)
			{
				if (!isServer)
				{
					return;
				}

				OnPickUp(character);
				_pickedUp.OnNext(Unit.Default);
				NetworkServer.Destroy(gameObject);
			}
			else
			{
				parachute.SetActive(false);
				IsLanded = true;
			}
		}

		private void ValidatePosition()
		{
			while (_mapProvider.Map.CurrentValue.HasIntersection(Bounds))
			{
				transform.position += Vector3.up;
			}
		}
		
		private ushort GetTopVoxelHeight(ushort x, ushort z)
		{
			var y = (ushort)(_mapProvider.Map.CurrentValue.Height - 1);

			if (_mapProvider.Map.CurrentValue.MapData.GetFace(x, y, z).HasFlag(Face.Top))
			{
				return y;
			}

			do
			{
				y--;

				if (_mapProvider.Map.CurrentValue.MapData.GetFace(x, y, z).HasFlag(Face.Top))
				{
					return y;
				}

			} while (y > 0);

			throw new InvalidOperationException($"Couldn't find top voxel at x={x}, z={z}");
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(Bounds.center, Bounds.size);
		}

		public Sprite MiniMapImage => miniMapImage;
		public override Bounds Bounds => new Bounds(localBounds.center + transform.position, localBounds.size);
		public bool IsLanded { get; private set; }
		public Observable<Unit> PickedUp => _pickedUp;

		protected virtual void OnPickUp(Character receiver)
		{
			_audioSender.SendAudio(AudioType.LootBoxPickUp, transform.position);
		}
	}
}