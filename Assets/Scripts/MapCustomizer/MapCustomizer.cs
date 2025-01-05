using System;
using System.Collections.Generic;
using System.Linq;
using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VoxelMap;
using Environment = VoxelMap.Environment;

namespace MapCustomizer
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class MapCustomizer : MonoBehaviour
    {
#if UNITY_EDITOR
        private const float Epsilon = 0.001f;
        public bool IsMapGenerated => _map != null;

        public MapConfigure mapConfigure;
        public List<GameObject> spawnPoints;
        private MapConfigure _previousConfigure;

        [SerializeField] private GameObject spawnPointPrefab;

        private static MapCustomizer _instance;
        private Map _map;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Init()
        {
            _instance = FindObjectOfType<MapCustomizer>();
        }

        private void Awake()
        {
            if (EditorApplication.isPlaying)
            {
                Destroy(gameObject);
                return;
            }

            if (_instance != null)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Update()
        {
            if (IsMapGenerated)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying
                    || _previousConfigure != mapConfigure)
                {
                    _map.Dispose();
                    DestroyImmediate(_map.gameObject);
                    return;
                }

                if (_map.DirectionalLight != null)
                {
                    CheckDirectionLightSource();
                }

                UpdateSpawnPointGameObjects();
            }

            _previousConfigure = mapConfigure;
        }

        private void CheckDirectionLightSource()
        {
            var stateChanged = false;

            if (mapConfigure.LightData.position != _map.DirectionalLight.transform.position)
            {
                mapConfigure.LightData.position = _map.DirectionalLight.transform.position;
                stateChanged = true;
            }

            if (mapConfigure.LightData.rotation != _map.DirectionalLight.transform.rotation)
            {
                mapConfigure.LightData.rotation = _map.DirectionalLight.transform.rotation;
                stateChanged = true;
            }

            if (mapConfigure.LightData.color != _map.DirectionalLight.color)
            {
                mapConfigure.LightData.color = _map.DirectionalLight.color;
                stateChanged = true;
            }

            if (Math.Abs(mapConfigure.LightData.bias - _map.DirectionalLight.shadowBias) > Epsilon)
            {
                mapConfigure.LightData.bias = _map.DirectionalLight.shadowBias;
                stateChanged = true;
            }

            if (Math.Abs(mapConfigure.LightData.normalBias - _map.DirectionalLight.shadowNormalBias) > Epsilon)
            {
                mapConfigure.LightData.normalBias = _map.DirectionalLight.shadowNormalBias;
                stateChanged = true;
            }

            if (stateChanged)
            {
                EditorUtility.SetDirty(mapConfigure);
            }
        }

        private void UpdateSpawnPointGameObjects()
        {
            if (spawnPoints.RemoveAll(obj => obj == null) > 0)
            {
                EditorUtility.SetDirty(mapConfigure);
                mapConfigure.SpawnPoints = spawnPoints.Select(obj =>
                        new SpawnPointData(Vector3Int.FloorToInt(obj.transform.localPosition)))
                    .ToList();
            }
            else
            {
                var newSpawnPoints =
                    spawnPoints
                        .Select(spawnPoint =>
                            new SpawnPointData(Vector3Int.FloorToInt(spawnPoint.transform.localPosition)))
                        .ToList();

                var spawnPointChanged = false;
                for (var i = 0; i < spawnPoints.Count; i++)
                {
                    if (newSpawnPoints[i] != mapConfigure.SpawnPoints[i])
                    {
                        spawnPointChanged = true;
                        break;
                    }
                }

                if (spawnPointChanged)
                {
                    EditorUtility.SetDirty(mapConfigure);
                }

                mapConfigure.SpawnPoints = newSpawnPoints;
            }
        }

        public async UniTask GenerateMap()
        {
            if (IsMapGenerated)
            {
                _map.Dispose();
                DestroyImmediate(_map.gameObject);
            }

            var assets = new AssetProvider();
            var mapBuilder = new MapBuilder(assets)
                .WithWaterColor(mapConfigure.WaterColor)
                .WithAmbient(mapConfigure.AmbientData)
                .WithDirectionalLight(mapConfigure.LightData)
                .WithFog(mapConfigure.FogData)
                .WithSkybox(mapConfigure.SkyboxMaterial)
                .WithWalls();

            _map = await mapBuilder.BuildAsync(MapReader.ReadFromFile(mapConfigure.name), transform);
            spawnPoints = mapConfigure.SpawnPoints.Select(spawnPoint => CreateSpawnPoint(spawnPoint.position))
                .ToList();
        }

        public void ShowAmbientLighting()
        {
            Environment.ApplyAmbientLighting(mapConfigure.AmbientData);
            Environment.ApplySkybox(mapConfigure.SkyboxMaterial);
        }

        public void ShowFog()
        {
            Environment.ApplyFog(mapConfigure.FogData);
        }

        public GameObject CreateSpawnPoint(Vector3Int position)
        {
            var spawnPoint = Instantiate(spawnPointPrefab, position, Quaternion.identity, _map.transform);
            return spawnPoint;
        }
#endif
    }
}