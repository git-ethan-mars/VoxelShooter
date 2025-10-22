using Data;
using GamePlay;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;
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
		private IMapConfigureLoader _mapConfigureLoader;

		private MapConfigure _mapConfigure;
		private RenderTexture _minimapTexture;

		[Inject]
		private void Construct(MapProvider mapProvider, IMapConfigureLoader mapConfigureLoader, 
			CharacterProvider characterProvider, EntityContainerService entityContainer)
		{
			_mapProvider = mapProvider;
			_mapConfigureLoader = mapConfigureLoader;
			_characterProvider = characterProvider;
			_entityContainer = entityContainer;

			Vector2 miniMapSize = minimapImage.rectTransform.rect.size;
			_minimapTexture = new RenderTexture((int)miniMapSize.x, (int)miniMapSize.y, 0, RenderTextureFormat.ARGB32)
			{
				enableRandomWrite = true,
				filterMode = FilterMode.Point
			};
		}

		private void Start()
		{
			_minimapTexture.Create();
			minimapImage.texture = _minimapTexture;
			_mapConfigure = _mapConfigureLoader.GetMapConfigure(_mapProvider.MapName);
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
			float zAngle = -Mathf.Atan2(_characterProvider.Character.Value.ForwardVector.x, _characterProvider.Character.Value
				.ForwardVector.z) * Mathf.Rad2Deg;
			minimapCursor.transform.rotation = Quaternion.Euler(0, 0, zAngle);
		}

		private void RedrawLootBoxes()
		{
			Vector3 characterPosition = _characterProvider.Character.Value.transform.position;
			var lootBoxesToDraw = 0;
			for (var i = 0; i < Items.Count; i++)
			{
				Items[i].gameObject.SetActive(false);
			}

			foreach (LootBox lootBox in _entityContainer.GetEntitiesByType<LootBox>())
			{
				if (IsLootBoxInsideMiniMap(lootBox.transform.position) && lootBox.IsLanded)
				{
					lootBoxesToDraw += 1;

					if (lootBoxesToDraw > Items.Count)
					{
						SpawnElement();
					}

					int imageIndex = lootBoxesToDraw - 1;
					Items[imageIndex].transform.localPosition = new Vector3(lootBox.transform.position.x - characterPosition.x,
						lootBox.transform.position.z - characterPosition.z);
					Items[imageIndex].sprite = lootBox.MiniMapImage;
					Items[imageIndex].gameObject.SetActive(true);
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