using Data;
using GamePlay;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;
using VoxelMap.Data;
using Image = UnityEngine.UI.Image;
namespace UI
{
	public class MiniMap : ListView<Image>
	{
		private const string DrawMiniMap = "DrawMiniMap";

		private static readonly int MapTexture = Shader.PropertyToID("MapTexture");
		private static readonly int MiniMapTexture = Shader.PropertyToID("MiniMapTexture");
		private static readonly int CharacterPosition = Shader.PropertyToID("CharacterPosition");
		private static readonly int FallbackColor = Shader.PropertyToID("FallbackColor");

		[SerializeField] private ComputeShader computeShader;
		[SerializeField] private WorldMap worldMap;
		[SerializeField] private RawImage minimapImage;
		[SerializeField] private Image minimapCursor;

		private MapProvider _mapProvider;
		private CharacterProvider _characterProvider;
		private EntityContainerService _entityContainer;

		private MapConfigure _mapConfigure;
		private RenderTexture _minimapTexture;

		[Inject]
		private void Construct(MapProvider mapProvider, CharacterProvider characterProvider, EntityContainerService entityContainer)
		{
			_mapProvider = mapProvider;
			_characterProvider = characterProvider;
			_entityContainer = entityContainer;

			Vector2 miniMapSize = minimapImage.rectTransform.rect.size;
			_minimapTexture = new RenderTexture((int)miniMapSize.x, (int)miniMapSize.y, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true,
				filterMode = FilterMode.Point
			};
		}

		public void Initialize()
		{
			_minimapTexture.Create();
			minimapImage.texture = _minimapTexture;
			_mapConfigure = _mapProvider.Map.MapConfigure;
		}

		private void Update()
		{
			if (_characterProvider.Character.Value == null)
			{
				return;
			}

			RedrawCursor();
			RedrawLootBoxes();
			RedrawMiniMap();
		}

		private void RedrawMiniMap()
		{
			int drawMiniMap = computeShader.FindKernel(DrawMiniMap);
			computeShader.SetTexture(drawMiniMap, MapTexture, worldMap.MainTexture);
			computeShader.SetTexture(drawMiniMap, MiniMapTexture, _minimapTexture);
			var characterPosition = _characterProvider.Character.Value.transform.position;
			computeShader.SetFloats(CharacterPosition, characterPosition.x, characterPosition.z);
			Color waterColor = _mapConfigure.WaterColor;
			computeShader.SetFloats(FallbackColor, waterColor.r, waterColor.g, waterColor.b, waterColor.a);
			computeShader.Dispatch(drawMiniMap,
				Mathf.CeilToInt((float)_minimapTexture.width / 8), Mathf.CeilToInt((float)_minimapTexture.height / 8),
				1);
		}

		private void RedrawCursor()
		{
			float zAngle = -Mathf.Atan2(_characterProvider.Character.Value.transform.forward.x, _characterProvider.Character.Value
				.transform.forward.z) * Mathf.Rad2Deg;
			minimapCursor.transform.rotation = Quaternion.Euler(0, 0, zAngle);
		}

		private void RedrawLootBoxes()
		{
			Vector3 characterPosition = _characterProvider.Character.Value.transform.position;

			Clear();

			foreach (LootBox lootBox in _entityContainer.GetEntitiesByType<LootBox>())
			{
				if (IsLootBoxInsideMiniMap(lootBox.transform.position) && lootBox.IsLanded)
				{
					Image image = SpawnElement();

					image.transform.localPosition = new Vector3(lootBox.transform.position.x - characterPosition.x,
						lootBox.transform.position.z - characterPosition.z);
					image.sprite = lootBox.MiniMapImage;
				}
			}
		}

		private bool IsLootBoxInsideMiniMap(Vector3 lootBoxPosition)
		{
			return Mathf.Abs(_characterProvider.Character.Value.transform.position.x - lootBoxPosition.x) < (float)_minimapTexture.width / 2 &&
			       Mathf.Abs(_characterProvider.Character.Value.transform.position.z - lootBoxPosition.z) < (float)_minimapTexture.height / 2;
		}

		private void OnDestroy()
		{
			_minimapTexture.Release();
		}
	}
}