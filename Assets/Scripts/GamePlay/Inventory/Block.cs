using Data;
using Mirror;
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
		private CameraProvider _cameraProvider;
		private CharacterProvider _characterProvider;
		private MapProvider _mapProvider;

		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, CharacterProvider characterProvider, MapProvider mapProvider)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_characterProvider = characterProvider;
			_mapProvider = mapProvider;
		}

		public override ItemType Type => ItemType.Block;
		
		private void Update()
		{
			if (!IsLocalItem)
			{
				return;
			}

			Character character = _characterProvider.Character.Value;
			float placeDistance = character.Characteristics.PlaceDistance;
			Color32 voxelColor = character.Inventory.DesiredVoxelColor.Value;
			
			if (_inputService.IsFirstActionButtonDown())
			{
				CmdBuild(_cameraProvider.CentredRay, voxelColor);
			}

			if (_cameraProvider.GetBuildRayCastHit(out RaycastHit hit, placeDistance))
			{
				var voxelPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2) + Map.WorldOffset;
				Graphics.DrawMesh(wireframeCube, Matrix4x4.TRS(voxelPosition, Quaternion.identity, Vector3.one * 1.001f),
					wireframeMaterial, 0);
				
				if (_inputService.IsScrollButtonDown())
				{
					Vector3Ushort colorPickingPosition = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);
					VoxelData colorPickingVoxel = _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(colorPickingPosition);
					character.Inventory.DesiredVoxelColor.Value = colorPickingVoxel.Color;
					Debug.Log(colorPickingVoxel.Color);
				}
			}
		}

		[Command]
		private void CmdBuild(Ray ray, Color32 voxelColor, NetworkConnectionToClient connection = null)
		{
			if (connection == null)
			{
				return;
			}

			var character = connection.identity.GetComponent<Character>();

			if (character == null)
			{
				return;
			}

			if (character.Inventory.VoxelAmount.CurrentValue <= 0)
			{
				return;
			}

			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, character.Characteristics.PlaceDistance, LayerMasks.AttackMask);

			if (!raycastResult)
			{
				return;
			}

			var buildVisitor = rayHit.collider.GetComponentInParent<IBuildVisitor>();
			
			if (buildVisitor != null && buildVisitor.Visit(this, rayHit, voxelColor))
			{
				character.Inventory.VoxelAmount.Value -= 1;
			}
		}
	}
}