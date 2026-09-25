using System.Collections.Generic;
using Data;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class Blueprint : InventoryItem
	{
		private const float ReachTolerance = 2.0f;

		private static readonly Color AvailableColor = new Color(0.0f, 1.0f, 0.0f, 0.2f);
		private static readonly Color UnavailableColor = new Color(1.0f, 0.0f, 0.0f, 0.2f);

		[SerializeField] protected MeshRenderer meshRenderer;

		[SerializeField] private MeshFilter meshFilter;

		private IInputService _inputService;
		private IStaticDataService _staticData;
		private CameraProvider _cameraProvider;
		private MapProvider _mapProvider;
		private CharacterProvider _characterProvider;

		[SyncVar(hook = nameof(OnLayoutChanged))]
		private int _layoutIndex;

		[SyncVar(hook = nameof(OnRotationChanged))]
		private int _rotationStep;

		private readonly Subject<BlueprintLayout> _layoutChanged = new Subject<BlueprintLayout>();
		private readonly List<Vector3Int> _positions = new List<Vector3Int>();
		private readonly List<Vector3Int> _linePositions = new List<Vector3Int>();
		private readonly List<Vector3Int> _lineOffsets = new List<Vector3Int>();
		private int _positionsLayoutIndex = -1;
		private int _positionsRotationStep = -1;
		private Vector3Int _minPosition;
		private Vector3Int _maxPosition;
		private Mesh _layoutMesh;
		private Mesh _lineMesh;
		private bool _isDrawingLine;
		private Vector3Int _lineStart;
		private Vector3Int _lineEnd;

		public override ItemType Type => ItemType.Blueprint;

		public new BlueprintConfigure Configure => base.Configure as BlueprintConfigure;
		public IReadOnlyList<BlueprintLayout> Layouts => _staticData.GetCharacteristics(OwnerClass).BlueprintLayouts;
		public int LayoutIndex => _layoutIndex;
		public BlueprintLayout CurrentLayout => Layouts[_layoutIndex];
		public Observable<BlueprintLayout> LayoutChanged => _layoutChanged;

		public IReadOnlyList<Vector3Int> Positions
		{
			get
			{
				RefreshPositions();
				return _positions;
			}
		}

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, CameraProvider cameraProvider,
			CharacterProvider characterProvider, MapProvider mapProvider)
		{
			_inputService = inputService;
			_staticData = staticData;
			_cameraProvider = cameraProvider;
			_characterProvider = characterProvider;
			_mapProvider = mapProvider;
		}

		public override void OnStartAuthority()
		{
			base.OnStartAuthority();
			_layoutMesh = CreateBlueprintMesh(Positions);
			meshFilter.mesh = _layoutMesh;
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
			bool hasHit = _cameraProvider.GetBuildRayCastHit(out RaycastHit hit, placeDistance);

			if (_inputService.IsRotateButtonDown())
			{
				CmdRotate();
			}

			if (_inputService.IsSecondActionButtonDown() && hasHit && CurrentLayout.CanBuildLine)
			{
				_isDrawingLine = true;
				_lineStart = GetLinePosition(hit);
				_lineEnd = _lineStart;
				RebuildLinePreview();
			}

			if (_isDrawingLine)
			{
				UpdateLine(hasHit, hit, character, voxelColor);
				return;
			}

			if (_inputService.IsFirstActionButtonDown())
			{
				CmdBuild(_cameraProvider.CentredRay, voxelColor);
			}

			if (hasHit)
			{
				Vector3Int blueprintPosition = GetPlacementPosition(hit);
				bool isAvailable = Positions.Count <= character.Inventory.VoxelAmount.CurrentValue &&
				                   IsAvailable(blueprintPosition, Positions);
				meshRenderer.materials[1].color = isAvailable ? AvailableColor : UnavailableColor;
				transform.position = blueprintPosition;
				meshRenderer.enabled = true;

				if (_inputService.IsScrollButtonDown())
				{
					var colorPickingPosition = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);
					VoxelData colorPickingVoxel = _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(colorPickingPosition);
					character.Inventory.DesiredVoxelColor.Value = colorPickingVoxel.Color;
				}
			}
			else
			{
				meshRenderer.enabled = false;
			}
		}

		private void OnDisable()
		{
			StopLine();
		}

		public void SelectLayout(int layoutIndex)
		{
			CmdSelectLayout(layoutIndex);
		}

		public Vector3Int GetPlacementPosition(RaycastHit hit)
		{
			RefreshPositions();
			var anchor = Vector3Int.FloorToInt(hit.point + hit.normal / 2);
			var normal = Vector3Int.RoundToInt(hit.normal);
			var offset = new Vector3Int(
				GetSurfaceOffset(normal.x, _minPosition.x, _maxPosition.x),
				GetSurfaceOffset(normal.y, _minPosition.y, _maxPosition.y),
				GetSurfaceOffset(normal.z, _minPosition.z, _maxPosition.z));
			return anchor + offset;
		}

		public override void Deselect()
		{
			base.Deselect();
			meshRenderer.enabled = false;
		}

		private static int GetSurfaceOffset(int normal, int min, int max)
		{
			if (normal > 0)
			{
				return -min;
			}

			if (normal < 0)
			{
				return -max;
			}

			return 0;
		}

		private static Vector3Int GetLinePosition(RaycastHit hit)
		{
			return Vector3Int.FloorToInt(hit.point + hit.normal / 2);
		}

		private void UpdateLine(bool hasHit, RaycastHit hit, Character character, Color32 voxelColor)
		{
			if (_inputService.IsSecondActionButtonUp())
			{
				CmdBuildLine(_lineStart, _lineEnd, voxelColor);
				StopLine();
				return;
			}

			// Input was disabled (a menu opened) while the button was held: drop the line.
			if (!_inputService.IsSecondActionButtonHold())
			{
				StopLine();
				return;
			}

			if (hasHit)
			{
				Vector3Int lineEnd = GetLinePosition(hit);

				if (lineEnd != _lineEnd)
				{
					_lineEnd = lineEnd;
					RebuildLinePreview();
				}
			}

			bool isAvailable = IsLineWithinReach(character, _lineStart, _lineEnd) &&
			                   _linePositions.Count <= character.Inventory.VoxelAmount.CurrentValue &&
			                   IsAvailable(Vector3Int.zero, _linePositions);
			meshRenderer.materials[1].color = isAvailable ? AvailableColor : UnavailableColor;
			transform.position = _lineStart;
			meshRenderer.enabled = true;
		}

		private void RebuildLinePreview()
		{
			VoxelLine.GetPositions(_lineStart, _lineEnd, Configure.MaxLineLength, _linePositions);
			_lineOffsets.Clear();

			foreach (Vector3Int position in _linePositions)
			{
				_lineOffsets.Add(position - _lineStart);
			}

			if (_lineMesh != null)
			{
				Destroy(_lineMesh);
			}

			_lineMesh = CreateBlueprintMesh(_lineOffsets);
			meshFilter.mesh = _lineMesh;
		}

		private void StopLine()
		{
			if (!_isDrawingLine)
			{
				return;
			}

			_isDrawingLine = false;
			_linePositions.Clear();
			meshFilter.mesh = _layoutMesh;
		}

		private bool IsAvailable(Vector3Int origin, IReadOnlyList<Vector3Int> offsets)
		{
			if (!_mapProvider.Map.CurrentValue.TryGetFeature(out MapBuilding mapBuilding))
			{
				return false;
			}

			foreach (Vector3Int offset in offsets)
			{
				if (!mapBuilding.IsAvailablePosition(origin + offset))
				{
					return false;
				}
			}

			return true;
		}

		[Command]
		private void CmdSelectLayout(int layoutIndex)
		{
			if (!IsSelected || layoutIndex < 0 || layoutIndex >= Layouts.Count)
			{
				return;
			}

			_layoutIndex = layoutIndex;
		}

		[Command]
		private void CmdRotate()
		{
			if (!IsSelected)
			{
				return;
			}

			_rotationStep = (_rotationStep + 1) % 4;
		}

		private void OnLayoutChanged(int oldLayoutIndex, int newLayoutIndex)
		{
			if (!CurrentLayout.CanBuildLine)
			{
				StopLine();
			}

			RebuildMeshIfOwned();
			_layoutChanged.OnNext(CurrentLayout);
		}

		private void OnRotationChanged(int oldRotationStep, int newRotationStep)
		{
			RebuildMeshIfOwned();
		}

		private void RebuildMeshIfOwned()
		{
			if (!isOwned)
			{
				return;
			}

			if (_layoutMesh != null)
			{
				Destroy(_layoutMesh);
			}

			_layoutMesh = CreateBlueprintMesh(Positions);

			if (!_isDrawingLine)
			{
				meshFilter.mesh = _layoutMesh;
			}
		}

		private void RefreshPositions()
		{
			if (_positionsLayoutIndex == _layoutIndex && _positionsRotationStep == _rotationStep)
			{
				return;
			}

			_positions.Clear();
			var rotation = Quaternion.Euler(0, 90 * _rotationStep, 0);
			var min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
			var max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

			foreach (Vector3Int position in CurrentLayout.Positions)
			{
				var rotatedPosition = Vector3Int.RoundToInt(rotation * position);
				_positions.Add(rotatedPosition);
				min = Vector3Int.Min(min, rotatedPosition);
				max = Vector3Int.Max(max, rotatedPosition);
			}

			_minPosition = min;
			_maxPosition = max;
			_positionsLayoutIndex = _layoutIndex;
			_positionsRotationStep = _rotationStep;
		}

		[Command]
		private void CmdBuild(Ray ray, Color32 voxelColor, NetworkConnectionToClient connection = null)
		{
			Character character = GetOwnerCharacter(connection);

			if (character == null || character.Inventory.VoxelAmount.CurrentValue < Positions.Count)
			{
				return;
			}

			// Same mask as the client preview, so entities in the way don't make the server reject a green preview.
			bool raycastResult = Physics.Raycast(ray, out RaycastHit rayHit, character.Characteristics.PlaceDistance, LayerMasks.BuildMask);

			if (!raycastResult)
			{
				return;
			}

			IBuildVisitor buildVisitor = rayHit.collider.GetComponentInParent<IBuildVisitor>();

			if (buildVisitor != null && buildVisitor.Visit(this, rayHit, voxelColor))
			{
				character.Inventory.VoxelAmount.Value -= Positions.Count;
			}
		}

		[Command]
		private void CmdBuildLine(Vector3Int start, Vector3Int end, Color32 voxelColor, NetworkConnectionToClient connection = null)
		{
			Character character = GetOwnerCharacter(connection);

			if (character == null || !IsSelected || !CurrentLayout.CanBuildLine)
			{
				return;
			}

			if (!IsLineWithinReach(character, start, end))
			{
				return;
			}

			var positions = new List<Vector3Int>();
			VoxelLine.GetPositions(start, end, Configure.MaxLineLength, positions);

			if (character.Inventory.VoxelAmount.CurrentValue < positions.Count ||
			    !_mapProvider.Map.CurrentValue.TryGetFeature(out MapBuilding mapBuilding) ||
			    !mapBuilding.Build(positions, voxelColor))
			{
				return;
			}

			character.Inventory.VoxelAmount.Value -= positions.Count;
		}

		// The end must be within reach; the start may trail behind by up to a full line, so the builder can walk while drawing.
		private bool IsLineWithinReach(Character character, Vector3Int start, Vector3Int end)
		{
			float reach = character.Characteristics.PlaceDistance + ReachTolerance;
			Vector3 characterPosition = character.transform.position;
			return Vector3.Distance(characterPosition, end + Map.WorldOffset) <= reach &&
			       Vector3.Distance(characterPosition, start + Map.WorldOffset) <= reach + Configure.MaxLineLength;
		}

		private static Character GetOwnerCharacter(NetworkConnectionToClient connection)
		{
			return connection?.identity != null ? connection.identity.GetComponent<Character>() : null;
		}

		private Mesh CreateBlueprintMesh(IReadOnlyList<Vector3Int> positions)
		{
			var mesh = new Mesh();
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var colors = new List<Color32>();
			var uv = new List<Vector2>();
			var triangles = new List<int>();
			var occupied = new HashSet<Vector3Int>(positions);

			foreach (Vector3Int voxelPosition in positions)
			{
				if (!occupied.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y + 1, voxelPosition.z)))
				{
					GenerateTopSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}

				if (!occupied.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y - 1, voxelPosition.z)))
				{
					GenerateBottomSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}

				if (!occupied.Contains(new Vector3Int(voxelPosition.x + 1, voxelPosition.y, voxelPosition.z)))
				{
					GenerateRightSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}

				if (!occupied.Contains(new Vector3Int(voxelPosition.x - 1, voxelPosition.y, voxelPosition.z)))
				{
					GenerateLeftSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}

				if (!occupied.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z + 1)))
				{
					GenerateFrontSide(voxelPosition.x, voxelPosition.y, voxelPosition.z, Color.white, vertices, normals, colors, uv, triangles);
				}

				if (!occupied.Contains(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z - 1)))
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

			for (int i = 0; i < 4; i++)
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

			for (int i = 0; i < 4; i++)
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

			for (int i = 0; i < 4; i++)
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

			for (int i = 0; i < 4; i++)
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

			for (int i = 0; i < 4; i++)
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

			for (int i = 0; i < 4; i++)
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
