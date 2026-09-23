using Cysharp.Threading.Tasks;
using Data;
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
		private CameraProvider _cameraProvider;
		private CharacterProvider _characterProvider;

		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		public ReactiveProperty<int> Amount => _amount;

		public override ItemType Type => ItemType.TNT;
		private float PlaceDistance => _characterProvider.Character.Value.Characteristics.PlaceDistance;
		private new TNTConfigure Configure => base.Configure as TNTConfigure;

		[Inject]
		private void Construct(IInputService inputService, IEntityFactory entityFactory,
			CameraProvider cameraProvider, IStaticDataService staticData, CharacterProvider characterProvider)
		{
			_inputService = inputService;
			_entityFactory = entityFactory;
			_cameraProvider = cameraProvider;
			_characterProvider = characterProvider;
		}

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
				PlaceTnt(_cameraProvider.CentredRay);
			}

			if (_cameraProvider.GetBuildRayCastHit(out RaycastHit rayCastHit, PlaceDistance))
			{
				Vector3 voxelCenter = Vector3Int.FloorToInt(rayCastHit.point - rayCastHit.normal / 2) + Map.WorldOffset;
				Vector3 position = voxelCenter + rayCastHit.normal / 2;
				var rotation = Quaternion.LookRotation(rayCastHit.normal == Vector3.up || rayCastHit.normal == Vector3.down ? Vector3.forward : Vector3.up, rayCastHit.normal);
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

			if (connection == null)
			{
				return;
			}

			Character character = connection.identity.GetComponent<Character>();

			if (character == null)
			{
				return;
			}

			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, character.Characteristics.PlaceDistance,
				LayerMasks.AttackMask);

			if (!raycastResult)
			{
				return;
			}

			Vector3 voxelCenter = Vector3Int.FloorToInt(rayHit.point - rayHit.normal / 2) + Map.WorldOffset;
			Vector3 position = voxelCenter + rayHit.normal / 2;
			var rotation = Quaternion.LookRotation(rayHit.normal == Vector3.up || rayHit.normal == Vector3.down ? Vector3.forward : Vector3.up, rayHit.normal);
			SpawningTNT tnt = _entityFactory.CreateSpawningTnt(position, rotation, connection);
			tnt.ExplodeAsync(tnt.destroyCancellationToken).Forget();
			_amount.Value -= 1;
		}
	}
}
