using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;
using Random = UnityEngine.Random;

namespace UI
{
	// Builds a random map behind the main menu and slowly circles the camera over it.
	public class MainMenuPanorama : MonoBehaviour
	{
		[SerializeField] private Image backgroundImage;
		[SerializeField] private float fadeDuration = 1.5f;
		[SerializeField] private float rotationSpeed = 3.0f;
		[SerializeField, Range(0, 0.5f)] private float orbitRadiusFactor = 0.35f;
		[SerializeField] private float cameraHeightFactor = 1.2f;
		[SerializeField, Range(0, 1)] private float lookHeightFactor = 0.3f;

		private IMapConfigureLoader _mapConfigureLoader;
		private Camera _camera;
		private Vector3 _center;
		private float _radius;
		private float _height;
		private float _angle;

		[Inject]
		private void Construct(IMapConfigureLoader mapConfigureLoader)
		{
			_mapConfigureLoader = mapConfigureLoader;
		}

		private void Start()
		{
			_camera = Camera.main;
			LoadMapAsync(destroyCancellationToken).Forget();
		}

		private void Update()
		{
			if (_radius == 0)
			{
				return;
			}

			_angle += rotationSpeed * Time.unscaledDeltaTime;
			Vector3 offset = Quaternion.Euler(0, _angle, 0) * new Vector3(0, _height, -_radius);
			_camera.transform.position = _center + offset;
			_camera.transform.LookAt(_center);
		}

		private async UniTaskVoid LoadMapAsync(CancellationToken token)
		{
			string[] mapNames = MapDataReader.GetExistedMaps().ToArray();

			if (_camera == null || mapNames.Length == 0)
			{
				return;
			}

			string mapName = mapNames[Random.Range(0, mapNames.Length)];
			Map map;

			try
			{
				MapData mapData = await MapDataReader.ReadFromFileAsync(mapName, token);
				map = await new MapBuilder()
					.FromConfigure(_mapConfigureLoader.GetMapConfigure(mapName))
					.BuildAsync(mapData, mapName, token: token);
			}
			catch (OperationCanceledException)
			{
				// The menu was left mid-build; the half-built map goes away with the scene.
				return;
			}

			DisableSceneDirectionalLights(map.transform);

			_center = new Vector3(map.Width / 2.0f, map.Height * lookHeightFactor, map.Depth / 2.0f);
			_radius = Mathf.Max(map.Width, map.Depth) * orbitRadiusFactor;
			_height = map.Height * cameraHeightFactor;
			_camera.farClipPlane = Mathf.Max(_camera.farClipPlane, Mathf.Max(map.Width, map.Depth) * 2);

			backgroundImage.DOFade(0, fadeDuration).SetLink(backgroundImage.gameObject);
		}

		// The map brings its own sun, so the menu scene light would double the lighting.
		private static void DisableSceneDirectionalLights(Transform mapTransform)
		{
			foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
			{
				if (light.type == LightType.Directional && !light.transform.IsChildOf(mapTransform))
				{
					light.enabled = false;
				}
			}
		}
	}
}
