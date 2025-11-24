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
		private CameraProvider _cameraProvider;
		private CharacterProvider _characterProvider;
		public RectPalette RectPalette { get; private set; }

		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, IStaticDataService staticData,
			CharacterProvider characterProvider, IPlayerService playerService)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_staticData = staticData;
			_characterProvider = characterProvider;
			_playerService = playerService;
			RectPalette = new RectPalette(staticData);
		}

		public override void OnStartServer()
		{
			base.OnStartServer();

			_amount.Value = Configure.Amount;
			_color.Value = RectPalette.SelectedColor;
		}

		public ReactiveProperty<int> Amount => _amount;
		public Color32 Color => _color.Value;
		public override ItemType Type => ItemType.Block;
		private new BlockConfigure Configure => base.Configure as BlockConfigure;
		private readonly SyncReactiveProperty<int> _amount = new SyncReactiveProperty<int>();
		private readonly SyncReactiveProperty<Color32> _color = new SyncReactiveProperty<Color32>();

		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			float placeDistance = _characterProvider.Character.Value.Characteristics.PlaceDistance;
			
			if (_inputService.IsFirstActionButtonDown())
			{
				CmdBuild(_cameraProvider.CentredRay);
			}

			if (_cameraProvider.GetBuildRayCastHit(out RaycastHit hit, placeDistance))
			{
				var voxelPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2) + Map.WorldOffset;
				Graphics.DrawMesh(wireframeCube, Matrix4x4.TRS(voxelPosition, Quaternion.identity, Vector3.one * 1.001f),
					wireframeMaterial, 0);
			}
			
			if (_inputService.IsUpArrowButtonDown())
			{
				RectPalette.MovePointerUp();
			}
			if (_inputService.IsDownArrowButtonDown())
			{
				RectPalette.MovePointerDown();
			}
			if (_inputService.IsRightArrowButtonDown())
			{
				RectPalette.MovePointerRight();
			}
			if (_inputService.IsLeftArrowButtonDown())
			{
				RectPalette.MovePointerLeft();
			}

			if (!RectPalette.SelectedColor.Equals(_color.Value))
			{
				for (var i = 0; i < model.Length; i++)
				{
					model[i].material.color = RectPalette.SelectedColor;
				}
				
				CmdChangeColor(RectPalette.SelectedColor);	
			}
		}

		[Command]
		private void CmdBuild(Ray ray, NetworkConnectionToClient connection = null)
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

		[Command]
		private void CmdChangeColor(Color32 color)
		{
			_color.Value = color;
		}
	}
}