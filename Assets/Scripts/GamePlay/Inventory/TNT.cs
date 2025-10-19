using Cysharp.Threading.Tasks;
using Data;
using GamePlay.Core;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
namespace GamePlay
{
	public class TNT : InventoryItem
	{
		[SerializeField] private Mesh tntMesh;
		[SerializeField] private Material tntMaterial;

		private IInputService _inputService;
		private IEntityFactory _entityFactory;
		private CameraService _cameraService;
		private CharacterProvider _characterProvider;
		private IStaticDataService _staticData;
		private IPlayerService _playerService;
		
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		public ReactiveProperty<int> Amount => _amount;
		private float PlaceDistance => _characterProvider.Character.Value.Characteristics.PlaceDistance;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory,
			CameraService cameraService, IStaticDataService staticData, CharacterProvider characterProvider,
			IPlayerService playerService)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraService = cameraService;
			_staticData = staticData;
			_characterProvider = characterProvider;
			_playerService = playerService;
		}

		public override ItemType Type => ItemType.TNT;
		private new TNTConfigure Configure => base.Configure as TNTConfigure;

		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
		}

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			if (_inputService.IsFirstActionButtonDown())
			{
				PlaceTnt(_cameraService.CentredRay);
			}
			
			if (_cameraService.GetBuildRayCastHit(out RaycastHit rayCastHit, PlaceDistance))
			{
				var voxelCenter = Vector3Int.FloorToInt(rayCastHit.point - rayCastHit.normal / 2) + Map.WorldOffset;
				Vector3 position = voxelCenter + rayCastHit.normal / 2;
				Quaternion rotation = Quaternion.LookRotation(rayCastHit.normal == Vector3.up || rayCastHit.normal == Vector3.down ?
					Vector3.forward : Vector3.up, rayCastHit.normal);
				Graphics.DrawMesh(tntMesh, Matrix4x4.TRS(position, rotation, Vector3.one), tntMaterial, 0);
			}
		}

		[Command]
		private void PlaceTnt(Ray ray, NetworkConnectionToClient connection = null)
		{
			if (_amount.Value <= 0)
			{
				return;
			}

			if (connection == null || !_playerService.TryGetPlayerData(connection.connectionId, out PlayerData playerData))
			{
				return;
			}

			Characteristics characteristics = _staticData.GetCharacteristics(playerData.GameClass);

			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, characteristics.PlaceDistance, LayerMasks.AttackMask);

			if (!raycastResult)
			{
				return;
			}

			Vector3 voxelCenter = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2) + Map.WorldOffset;
			Vector3 position = voxelCenter + rayHit.normal / 2;
			Quaternion rotation = Quaternion.LookRotation(rayHit.normal == Vector3.up || rayHit.normal == Vector3.down ?
				Vector3.forward : Vector3.up, rayHit.normal);
			SpawningTNT tnt = _entityFactory.CreateSpawningTnt(position, rotation);
			tnt.ExplodeAsync(tnt.destroyCancellationToken).Forget();
			_amount.Value -= 1;
		}
	}
}