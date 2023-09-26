using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using Generators;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Optimization;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class MapCustomizer : MonoBehaviour, ICoroutineRunner
{
    public bool IsMapGenerated => chunkContainer != null || spawnPointsContainer != null;
    public MapConfigure mapConfigure;
    public Light lightSource;
    public List<GameObject> spawnPoints;

    [SerializeField]
    private GameObject chunkContainer;

    [SerializeField]
    private GameObject spawnPointsContainer;

    private StaticDataService _staticData;
    private MeshFactory _meshFactory;
    private GameFactory _gameFactory;
    private EntityFactory _entityFactory;
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
        _instance._mapProvider =
            SimpleBenchmark.Execute(MapReader.ReadFromFile, mapConfigure.name, _instance._staticData);
        var mapGenerator =
            new MapGenerator(_instance._mapProvider, _instance._gameFactory, _instance._meshFactory);
        SimpleBenchmark.Execute(mapGenerator.GenerateMap);
        chunkContainer = mapGenerator.ChunkContainer;
        chunkContainer.transform.SetParent(transform);
        spawnPoints.Clear();
        spawnPointsContainer = _instance._gameFactory.CreateGameObjectContainer("SpawnPointsContainer");
        spawnPointsContainer.transform.SetParent(transform);

        for (var i = 0; i < mapConfigure.spawnPoints.Count; i++)
        {
            spawnPoints.Add(CreateSpawnPoint(mapConfigure.spawnPoints[i].position));
        }
    }

    public GameObject CreateSpawnPoint(Vector3Int position)
    {
        var spawnPoint = _instance._entityFactory.CreateSpawnPoint(position, spawnPointsContainer.transform);
        spawnPoint.AddComponent<PositionAligner>();
        return spawnPoint;
    }

    private void InitializeFactories()
    {
        var assets = new AssetProvider();
        _instance._entityFactory = new EntityFactory(assets);
        _instance._staticData = new StaticDataService();
        _instance._staticData.LoadMapConfigures();
        var particleFactory = new ParticleFactory(assets, this);
        _instance._meshFactory = new MeshFactory(assets);
        _instance._gameFactory =
            new GameFactory(assets, _entityFactory, _staticData, particleFactory, _meshFactory);
    }

    private void LoadConfigure()
    {
        if (lightSource != null)
        {
            lightSource.transform.position = mapConfigure.lightData.position;
            lightSource.transform.rotation = mapConfigure.lightData.rotation;
            lightSource.color = mapConfigure.lightData.color;
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

        DestroyImmediate(chunkContainer);
        DestroyImmediate(spawnPointsContainer);
    }

    public void ShowAmbientLighting()
    {
        Environment.ApplyAmbientLighting(mapConfigure);
    }

    public void ShowFog()
    {
        Environment.ApplyFog(mapConfigure);
    }
}