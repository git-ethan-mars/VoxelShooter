using System;
using Mirror;
using Networking.Audio;
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

		private static Transform _lootBoxContainer;

		[SerializeField] private Sprite miniMapImage;
		[SerializeField] private new Collider collider;
		[SerializeField] private GameObject parachute;
		[SerializeField] private GameObject platformPrefab;
		[SerializeField] private Bounds localBounds;

		private readonly Subject<Unit> _pickedUp = new Subject<Unit>();
		
		private EntityContainerService _entityContainer;
		private MapProvider _mapProvider;
		private IAssetProvider _assets;
		private NetworkAudioPlayer _audioPlayer;

		private GameObject _platform;

		[Inject]
		private void Construct(EntityContainerService entityContainer, MapProvider mapProvider, IAssetProvider assets, NetworkAudioPlayer audioPlayer)
		{
			_entityContainer = entityContainer;
			_mapProvider = mapProvider;
			_assets = assets;
			_audioPlayer = audioPlayer;
		}

		private void Start()
		{
			_entityContainer.Add(this);

			if (_lootBoxContainer == null)
			{
				_lootBoxContainer = new GameObject(LootBoxContainer).transform;
			}

			transform.SetParent(_lootBoxContainer.transform);

			int platformPositionX = Mathf.FloorToInt(transform.position.x);
			int platformPositionZ = Mathf.FloorToInt(transform.position.z);
			int platformPositionY = GetTopVoxelHeight((ushort)platformPositionX, (ushort)platformPositionZ);
			Vector3 platformPosition = new Vector3(platformPositionX, platformPositionY, platformPositionZ) + Map.WorldOffset + Vector3.up * 0.5f;
			_platform = _assets.Instantiate(platformPrefab, _lootBoxContainer);
			_platform.transform.position = platformPosition;
		}

		private void OnDestroy()
		{
			_audioPlayer.Play(AudioType.LootBoxPickUp, transform.position).Forget();
			_entityContainer.Remove(this);

			if (_platform != null)
			{
				Destroy(_platform);
			}
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_mapProvider.Map.MapUpdated
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
			while (_mapProvider.Map.HasIntersection(Bounds))
			{
				transform.position += Vector3.up;
			}
		}
		
		private ushort GetTopVoxelHeight(ushort x, ushort z)
		{
			var y = (ushort)(_mapProvider.Map.Height - 1);

			if (_mapProvider.Map.MapData.GetFace(x, y, z).HasFlag(Face.Top))
			{
				return y;
			}

			do
			{
				y--;

				if (_mapProvider.Map.MapData.GetFace(x, y, z).HasFlag(Face.Top))
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
		protected abstract void OnPickUp(Character receiver);
	}
}