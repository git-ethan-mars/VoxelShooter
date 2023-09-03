using Generators;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class MapEditor : MonoBehaviour, ICoroutineRunner
{
    public Light lightSource;

    public Material skybox;

    public MapConfigure mapConfigure;

    private MapGenerator _mapGenerator;
    private MapProvider _mapProvider;
    private MeshFactory _meshFactory;
    private GameFactory _gameFactory;

    private static MapEditor _instance;

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void Init()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<MapEditor>();
        }
    }

    private void Awake()
    {
        if (EditorApplication.isPlaying)
        {
            Destroy(gameObject);
        }

        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        var assets = new AssetProvider();
        var entityFactory = new EntityFactory(assets);
        var staticData = new StaticDataService();
        var particleFactory = new ParticleFactory(assets, this);
        _meshFactory = new MeshFactory(assets);
        _gameFactory = new GameFactory(assets, entityFactory, staticData, particleFactory, _meshFactory);
    }

    private void Update()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            DestroyChunks();
        }
    }

    public void GenerateMap()
    {
        DestroyChunks();
        _mapProvider = MapReader.ReadFromFile(mapConfigure.mapName);
        _mapGenerator = new MapGenerator(_mapProvider, _gameFactory, _meshFactory);
        _mapGenerator.GenerateMap();
        _mapGenerator.Dispose();
        _mapProvider.Dispose();
    }

    public void SaveLighting()
    {
        mapConfigure.lightPosition = lightSource.transform.position;
        mapConfigure.lightRotation = lightSource.transform.rotation;
        mapConfigure.lightColor = lightSource.color;
    }

    public void SaveSkybox()
    {
        mapConfigure.skyboxMaterial = skybox;
        RenderSettings.skybox = mapConfigure.skyboxMaterial;
    }

    private void DestroyChunks()
    {
        if (_mapGenerator != null)
        {
            DestroyImmediate(_mapGenerator.ChunkContainer);
        }
    }
}