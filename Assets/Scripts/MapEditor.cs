using System.Collections.Generic;
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
public class MapEditor : MonoBehaviour, ICoroutineRunner
{
    public bool IsMapGenerated { get; private set; }
    public MapConfigure mapConfigure;
    public Color32 waterColor;
    public Color32 innerColor;
    public Light lightSource;
    public Material skybox;
    public List<GameObject> spawnPoints;

    [SerializeField]
    private GameObject chunkContainer;

    [SerializeField]
    private GameObject spawnPointsContainer;

    private StaticDataService _staticData;
    private MeshFactory _meshFactory;
    private GameFactory _gameFactory;
    private EntityFactory _entityFactory;
    private static MapEditor _instance;
    private MapConfigure _previousConfigure;

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void Init()
    {
        _instance = FindObjectOfType<MapEditor>();
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

        if (IsMapGenerated)
        {
            mapConfigure.waterColor = waterColor;
            mapConfigure.innerColor = innerColor;
            mapConfigure.skyboxMaterial = skybox;
            spawnPoints.RemoveAll(obj => obj == null);
            mapConfigure.spawnPoints = spawnPoints
                .Select(obj => new SpawnPointData(Vector3Int.FloorToInt(obj.transform.localPosition))).ToList();
        }
    }

    public void GenerateMap()
    {
        DestroyChildren();
        LoadConfigure();
        var mapProvider =
            SimpleBenchmark.Execute(MapReader.ReadFromFile, mapConfigure.mapName, _instance._staticData);
        var mapGenerator =
            new MapGenerator(mapProvider, _instance._gameFactory, _instance._meshFactory, _instance._staticData);
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

        IsMapGenerated = true;
    }

    public void SaveLighting()
    {
        mapConfigure.lightData = new LightData(lightSource.transform.position, lightSource.transform.rotation,
            lightSource.color);
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
        waterColor = mapConfigure.waterColor;
        innerColor = mapConfigure.innerColor;
        skybox = mapConfigure.skyboxMaterial;
    }

    private void DestroyChildren()
    {
        if (!IsMapGenerated)
        {
            return;
        }

        DestroyImmediate(chunkContainer);
        DestroyImmediate(spawnPointsContainer);
        IsMapGenerated = false;
    }
}