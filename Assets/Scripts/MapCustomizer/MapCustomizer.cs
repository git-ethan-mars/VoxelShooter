using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Generators;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using MapLogic;
using Optimization;
using UnityEditor;
using UnityEngine;
using Environment = MapLogic.Environment;

namespace MapCustomizer
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class MapCustomizer : MonoBehaviour, ICoroutineRunner
    {
#if UNITY_EDITOR
        private const string ChunkContainerName = "ChunkContainer";
        private const string SpawnPointsContainer = "SpawnPointsContainer";

        public bool IsMapGenerated => chunkContainer != null || spawnPointsContainer != null;
        public MapConfigure mapConfigure;
        public Light lightSource;
        public List<GameObject> spawnPoints;

        [SerializeField]
        private Transform chunkContainer;

        [SerializeField]
        private Transform spawnPointsContainer;

        private static MapCustomizer _instance;
        private MapConfigure _previousConfigure;
        private MapProvider _mapProvider;

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void Init()
        {
            _instance = FindObjectOfType<MapCustomizer>();
            if (_instance != null)
            {
                _instance.InitializeFactories();
            }
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
                _instance.InitializeFactories();
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                DestroyChildren();
                return;
            }

            if (_previousConfigure != mapConfigure)
            {
                DestroyChildren();
            }

            _previousConfigure = mapConfigure;

            if (mapConfigure == null)
            {
                return;
            }

            if (!IsMapGenerated)
            {
                return;
            }

            if (lightSource != null)
            {
                if (mapConfigure.lightData.position != lightSource.transform.position)
                {
                    mapConfigure.lightData.position = lightSource.transform.position;
                    EditorUtility.SetDirty(mapConfigure);
                }

                if (mapConfigure.lightData.rotation != lightSource.transform.rotation)
                {
                    mapConfigure.lightData.rotation = lightSource.transform.rotation;
                    EditorUtility.SetDirty(mapConfigure);
                }

                if (mapConfigure.lightData.color != lightSource.color)
                {
                    mapConfigure.lightData.color = lightSource.color;
                    EditorUtility.SetDirty(mapConfigure);
                }

                if (Math.Abs(mapConfigure.lightData.bias - lightSource.shadowBias) > Constants.Epsilon)
                {
                    mapConfigure.lightData.bias = lightSource.shadowBias;
                    EditorUtility.SetDirty(mapConfigure);
                }

                if (Math.Abs(mapConfigure.lightData.normalBias - lightSource.shadowNormalBias) > Constants.Epsilon)
                {
                    mapConfigure.lightData.normalBias = lightSource.shadowNormalBias;
                    EditorUtility.SetDirty(mapConfigure);
                }
            }

            UpdateSpawnPointGameObjects();
        }

        private void UpdateSpawnPointGameObjects()
        {
            if (spawnPoints.RemoveAll(obj => obj == null) > 0)
            {
                EditorUtility.SetDirty(mapConfigure);
                mapConfigure.spawnPoints = spawnPoints.Select(obj =>
                        new SpawnPointData(Vector3Int.FloorToInt(obj.transform.localPosition)))
                    .ToList();
            }
            else
            {
                var newSpawnPoints = new List<SpawnPointData>(spawnPoints.Count);
                for (var i = 0; i < spawnPoints.Count; i++)
                {
                    newSpawnPoints.Add(
                        new SpawnPointData(Vector3Int.FloorToInt(spawnPoints[i].transform.localPosition)));
                    if (!newSpawnPoints[i].Equals(mapConfigure.spawnPoints[i]))
                    {
                        EditorUtility.SetDirty(mapConfigure);
                    }
                }

                mapConfigure.spawnPoints = newSpawnPoints;
            }
        }

        public void GenerateMap()
        {
            DestroyChildren();
            LoadConfigure();
            AllServices.Container.Single<IStaticDataService>().LoadMapConfigures();
            _instance._mapProvider =
                SimpleBenchmark.Execute(MapReader.ReadFromFile, mapConfigure.name,
                    AllServices.Container.Single<IStaticDataService>());
            var mapGenerator =
                new MapGenerator(_instance._mapProvider, AllServices.Container.Single<IGameFactory>(),
                    AllServices.Container.Single<IMeshFactory>());
            chunkContainer = AllServices.Container.Single<IGameFactory>().CreateGameObjectContainer(ChunkContainerName);
            chunkContainer.SetParent(transform);
            SimpleBenchmark.Execute(mapGenerator.GenerateMap, chunkContainer);
            spawnPoints.Clear();
            spawnPointsContainer = AllServices.Container.Single<IGameFactory>()
                .CreateGameObjectContainer(SpawnPointsContainer);
            spawnPointsContainer.SetParent(transform);

            for (var i = 0; i < mapConfigure.spawnPoints.Count; i++)
            {
                spawnPoints.Add(CreateSpawnPoint(mapConfigure.spawnPoints[i].position));
            }
        }

        public GameObject CreateSpawnPoint(Vector3Int position)
        {
            var spawnPoint = AllServices.Container.Single<IEntityFactory>()
                .CreateSpawnPoint(position, spawnPointsContainer)
                .gameObject;
            spawnPoint.AddComponent<PositionAligner>();
            return spawnPoint;
        }

        private void InitializeFactories()
        {
            AllServices.Container.RegisterSingle<IAssetProvider>(new AssetProvider());
            var assets = AllServices.Container.Single<IAssetProvider>();
            AllServices.Container.RegisterSingle<IStaticDataService>(new StaticDataService(assets));
            var staticData = AllServices.Container.Single<IStaticDataService>();
            AllServices.Container.RegisterSingle<IInputService>(new StandaloneInputService());
            AllServices.Container.RegisterSingle<IStorageService>(new JsonToFileStorageService());
            AllServices.Container.RegisterSingle<IParticleFactory>(new ParticleFactory(assets, staticData, this));
            AllServices.Container.RegisterSingle<IEntityFactory>(
                new EntityFactory(assets, AllServices.Container.Single<IParticleFactory>()));
            AllServices.Container.RegisterSingle<IMeshFactory>(new MeshFactory(assets));
            AllServices.Container.RegisterSingle<IUIFactory>(new UIFactory(assets, staticData));
            AllServices.Container.RegisterSingle<IGameFactory>(new GameFactory(AllServices.Container));
        }

        private void LoadConfigure()
        {
            if (lightSource != null)
            {
                lightSource.transform.position = mapConfigure.lightData.position;
                lightSource.transform.rotation = mapConfigure.lightData.rotation;
                lightSource.color = mapConfigure.lightData.color;
                lightSource.shadowBias = mapConfigure.lightData.bias;
                lightSource.shadowNormalBias = mapConfigure.lightData.normalBias;
            }

            Environment.ApplyAmbientLighting(mapConfigure);
            Environment.ApplyFog(mapConfigure);
        }

        private void DestroyChildren()
        {
            if (!IsMapGenerated)
            {
                return;
            }

            DestroyImmediate(chunkContainer.gameObject);
            DestroyImmediate(spawnPointsContainer.gameObject);
        }

        public void ShowAmbientLighting()
        {
            Environment.ApplyAmbientLighting(mapConfigure);
        }

        public void ShowFog()
        {
            Environment.ApplyFog(mapConfigure);
        }
#endif
    }
}