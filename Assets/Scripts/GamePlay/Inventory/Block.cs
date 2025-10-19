using Data;
using GamePlay.Core;
using GamePlay.MapFeatures;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class Block : InventoryItem
	{
		[SerializeField] protected Material wireframeMaterial;
		[SerializeField] protected Mesh wireframeCube;

		private IInputService _inputService;
		private IStaticDataService _staticData;
		private IPlayerService _playerService;
		private CameraService _cameraService;
		private CharacterProvider _characterProvider;

		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, IStaticDataService staticData,
			CharacterProvider characterProvider, IPlayerService playerService)
		{
			_inputService = inputService;
			_cameraService = cameraService;
			_staticData = staticData;
			_characterProvider = characterProvider;
			_playerService = playerService;
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
		}

		public ReactiveProperty<int> Amount => _amount;
		public Color32 SelectedColor { get; set; }
		public override ItemType Type => ItemType.Block;
		private new BlockConfigure Configure => base.Configure as BlockConfigure;
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			float placeDistance = _characterProvider.Character.Value.Characteristics.PlaceDistance;
			
			if (_inputService.IsFirstActionButtonDown())
			{
				Build(_cameraService.CentredRay);
			}

			if (_cameraService.GetBuildRayCastHit(out RaycastHit hit, placeDistance))
			{
				var voxelPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2) + Map.WorldOffset;
				Graphics.DrawMesh(wireframeCube, Matrix4x4.TRS(voxelPosition, Quaternion.identity, Vector3.one * 1.001f),
					wireframeMaterial, 0);
			}
		}

		[Command]
		private void Build(Ray ray, NetworkConnectionToClient connection = null)
		{
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

			var buildVisitor = rayHit.collider.GetComponentInParent<IBuildVisitor>();
			buildVisitor?.Visit(this, rayHit);
		}
	}
}