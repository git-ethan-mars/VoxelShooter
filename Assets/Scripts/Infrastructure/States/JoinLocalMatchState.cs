using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using Networking;
using Networking.Client;
using UI;
using UnityEngine;
using VoxelMap;

namespace Infrastructure.States
{
    public class JoinLocalMatchState : IState
    {
        private const string Main = "Main";

        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IAssetProvider _assets;
        private readonly INetworkingFactory _networkingFactory;
        private readonly IUIFactory _uiFactory;
        private readonly IMapConfigureLoader _mapConfigureLoader;
        private IClient _client;
        private LoadingWindow _loadingWindow;


        public JoinLocalMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, IAssetProvider assets, INetworkingFactory
            networkingFactory, IUIFactory uiFactory, IMapConfigureLoader mapConfigureLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _assets = assets;
            _networkingFactory = networkingFactory;
            _uiFactory = uiFactory;
            _mapConfigureLoader = mapConfigureLoader;
        }


        public void Enter()
        {
            _sceneLoader.Load(Main, OnLoaded);
        }

        private void OnLoaded()
        {
            _loadingWindow = _uiFactory.CreateLoadingWindow();
            _client = _networkingFactory.CreateClient();
            _client.MapLoaded += OnMapLoaded;
            _client.MapLoadProgressed += _loadingWindow.UpdateLoadingBar;
            _client.Start();
        }

        private async void OnMapLoaded(string mapName, MapData mapData)
        {
            _client.MapLoaded -= OnMapLoaded;
            _client.MapLoadProgressed -= _loadingWindow.UpdateLoadingBar;
            Object.Destroy(_loadingWindow);
            var mapConfigure = _mapConfigureLoader.GetMapConfigure(mapName);
            var mapBuilder = new MapBuilder(_assets)
                .WithWaterColor(mapConfigure.WaterColor)
                .WithInnerColor(mapConfigure.InnerColor)
                .WithAmbient(mapConfigure.AmbientData)
                .WithDirectionalLight(mapConfigure.LightData)
                .WithFog(mapConfigure.FogData)
                .WithSkybox(mapConfigure.SkyboxMaterial)
                .WithWalls();
            var map = await mapBuilder.BuildAsync(mapData);
            var mapProvider = new MapProvider(map);
            _stateMachine.Enter<GameLoopState, (IClient, MapProvider)>((_client, mapProvider));
        }

        public void Exit()
        {
        }
    }
}