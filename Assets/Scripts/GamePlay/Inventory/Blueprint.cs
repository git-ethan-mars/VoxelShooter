using System.Collections.Generic;
using System.Linq;
using Data;
using GamePlay.Audio;
using Mirror;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;
using AudioType = Data.AudioType;
namespace GamePlay
{
	public class Blueprint : InventoryItem
	{
		public override ItemType Type => ItemType.Blueprint;

		[SerializeField] private MeshFilter meshFilter;
		[SerializeField] protected MeshRenderer meshRenderer;

		private IInputService _inputService;
		private CameraProvider _cameraProvider;
		private MapProvider _mapProvider;
		private CharacterProvider _characterProvider;
		private AudioPlayer _audioPlayer;

		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, CharacterProvider characterProvider, 
			MapProvider mapProvider, AudioPlayer audioPlayer)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_characterProvider = characterProvider;
			_mapProvider = mapProvider;
			_audioPlayer = audioPlayer;
		}

		public new BlueprintConfigure Configure => base.Configure as BlueprintConfigure;

		public override void OnStartAuthority()
		{
			base.OnStartAuthority();
			meshFilter.mesh = CreateBlueprintMesh();
		}

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
				Vector3Int blueprintPosition = GetPlacementPosition(hit);
				var buildVisitor = hit.collider.GetComponentInParent<IBuildVisitor>();

				if (Configure.Positions.Any(offset => !buildVisitor.IsAvailablePosition(blueprintPosition + offset)))
				{
					meshRenderer.materials[1].color = new Color(1, 0, 0, 0.2f);
				}
				else
				{
					meshRenderer.materials[1].color = new Color(0, 1, 0, 0.2f);
				}

				transform.position = blueprintPosition;
				meshRenderer.enabled = true;
				
				if (_inputService.IsScrollButtonDown())
				{
					Vector3Ushort colorPickingPosition = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);
					VoxelData colorPickingVoxel = _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(colorPickingPosition);
					character.Inventory.DesiredVoxelColor.Value = colorPickingVoxel.Color;
					Debug.Log(colorPickingVoxel.Color);
				}
			}
			else
			{
				meshRenderer.enabled = false;
			}
		}

		public Vector3Int GetPlacementPosition(RaycastHit hit)
		{
			Vector3 boundOffset = Vector3.Scale(hit.normal, meshFilter.mesh.bounds.extents);
			Vector3Int blueprintPosition = Vector3Int.FloorToInt(hit.point + hit.normal / 2)
			                               + new Vector3Int((int)boundOffset.x, (int)boundOffset.y, (int)boundOffset.z);
			return blueprintPosition;
		}

		public override void Deselect()
		{
			base.Deselect();
			meshRenderer.enabled = false;
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

			if (character.Inventory.VoxelAmount.CurrentValue < Configure.Positions.Count)
			{
				PlayDeniedSound();
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
				character.Inventory.VoxelAmount.Value -= Configure.Positions.Count;
			}
			else
			{
				PlayDeniedSound();
			}
		}

		[TargetRpc]
		private void PlayDeniedSound()
		{
			_audioPlayer.Play(AudioType.ShotgunShoot, transform.position).Forget();
		}

		private Mesh CreateBlueprintMesh()
		{
			var mesh = new Mesh();
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var colors = new List<Color32>();
			var uv = new List<Vector2>();
			var triangles = new List<int>();

			for (var i = 0; i < Configure.Positions.Count; i++)
			{
				Vector3Int voxelPosition = Configure.Positions[i];

				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y + 1, voxelPosition.z)))
				{
					GenerateTopSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y - 1, voxelPosition.z)))
				{
					GenerateBottomSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x + 1, voxelPosition.y, voxelPosition.z)))
				{
					GenerateRightSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x - 1, voxelPosition.y, voxelPosition.z)))
				{
					GenerateLeftSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z + 1)))
				{
					GenerateFrontSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
				if (!Configure.Positions.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z - 1)))
				{
					GenerateBackSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}
			}

			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetColors(colors);
			mesh.SetUVs(0, uv);
			mesh.SetTriangles(triangles, 0);
			mesh.RecalculateBounds();

			return mesh;
		}

		private void GenerateTopSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors, 
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x, y + 1, z));
			vertices.Add(new Vector3(x, y + 1, z + 1));
			vertices.Add(new Vector3(x + 1, y + 1, z));
			vertices.Add(new Vector3(x + 1, y + 1, z + 1));
			
			uv.Add(new Vector2(0, 1));
			uv.Add(new Vector2(0, 0));
			uv.Add(new Vector2(1, 1));
			uv.Add(new Vector2(1, 0));

			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.up);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void GenerateBottomSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors,
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x, y, z));
			vertices.Add(new Vector3(x + 1, y, z));
			vertices.Add(new Vector3(x, y, z + 1));
			vertices.Add(new Vector3(x + 1, y, z + 1));
			
			uv.Add(new Vector2(1, 1));
			uv.Add(new Vector2(0, 1));
			uv.Add(new Vector2(1, 0));
			uv.Add(new Vector2(0, 0));

			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.down);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void GenerateFrontSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors,
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x, y, z + 1));
			vertices.Add(new Vector3(x + 1, y, z + 1));
			vertices.Add(new Vector3(x, y + 1, z + 1));
			vertices.Add(new Vector3(x + 1, y + 1, z + 1));

			uv.Add(new Vector2(1, 0));
			uv.Add(new Vector2(0, 0));
			uv.Add(new Vector2(1, 1));
			uv.Add(new Vector2(0, 1));
			
			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.forward);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void GenerateBackSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors,
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x, y, z));
			vertices.Add(new Vector3(x, y + 1, z));
			vertices.Add(new Vector3(x + 1, y, z));
			vertices.Add(new Vector3(x + 1, y + 1, z));
			
			uv.Add(new Vector2(0, 0));
			uv.Add(new Vector2(0, 1));
			uv.Add(new Vector2(1, 0));
			uv.Add(new Vector2(1, 1));

			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.back);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void GenerateRightSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors,
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x + 1, y, z));
			vertices.Add(new Vector3(x + 1, y + 1, z));
			vertices.Add(new Vector3(x + 1, y, z + 1));
			vertices.Add(new Vector3(x + 1, y + 1, z + 1));
			
			uv.Add(new Vector2(0, 0));
			uv.Add(new Vector2(0, 1));
			uv.Add(new Vector2(1, 0));
			uv.Add(new Vector2(1, 1));

			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.right);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void GenerateLeftSide(int x, int y, int z, Color32 color, List<Vector3> vertices, List<Vector3> normals, List<Color32> colors,
			List<Vector2> uv, List<int> triangles)
		{
			vertices.Add(new Vector3(x, y, z));
			vertices.Add(new Vector3(x, y, z + 1));
			vertices.Add(new Vector3(x, y + 1, z));
			vertices.Add(new Vector3(x, y + 1, z + 1));

			uv.Add(new Vector2(1, 0));
			uv.Add(new Vector2(0, 0));
			uv.Add(new Vector2(1, 1));
			uv.Add(new Vector2(0, 1));
			
			for (var i = 0; i < 4; i++)
			{
				normals.Add(Vector3.left);
				colors.Add(color);
			}

			AddTriangles(triangles, vertices.Count);
		}

		private void AddTriangles(List<int> triangles, int vertexCount)
		{
			triangles.Add(vertexCount - 4);
			triangles.Add(vertexCount - 3);
			triangles.Add(vertexCount - 2);
			triangles.Add(vertexCount - 3);
			triangles.Add(vertexCount - 1);
			triangles.Add(vertexCount - 2);
		}
	}
}