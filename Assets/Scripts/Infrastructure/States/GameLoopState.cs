using Data;
using Generators;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using MapLogic;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<CustomNetworkManager>
    {
        private const string ChunkContainerName = "ChunkContainer";

        private readonly GameStateMachine _gameStateMachine;
        private readonly IStaticDataService _staticData;
        private readonly IGameFactory _gameFactory;
        private readonly IUIFactory _uiFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IAvatarLoader _avatarLoader;
        private MapMeshUpdater _mapMeshUpdater;
        private VerticalMapProjector _mapProjector;
        private CustomNetworkManager _networkManager;

        public GameLoopState(GameStateMachine gameStateMachine, IStaticDataService staticData, IGameFactory gameFactory, IUIFactory uiFactory,
            IMeshFactory meshFactory, IInputService inputService,
            IStorageService storageService,
            IAvatarLoader avatarLoader)
        {
            _gameStateMachine = gameStateMachine;
            _staticData = staticData;
            _gameFactory = gameFactory;
            _uiFactory = uiFactory;
            _meshFactory = meshFactory;
            _inputService = inputService;
            _storageService = storageService;
            _avatarLoader = avatarLoader;
        }

        public void Enter(CustomNetworkManager networkManager)
        {
            _networkManager = networkManager;
            var mapGenerator = new MapGenerator(networkManager.Client.MapProvider, _gameFactory, _meshFactory);
            var chunkMeshes = mapGenerator.GenerateMap(_gameFactory.CreateGameObjectContainer(ChunkContainerName));
            _mapMeshUpdater = new MapMeshUpdater(chunkMeshes, networkManager.Client.MapProvider);
            var mapConfigure = _staticData.GetMapConfigure(networkManager.Client.MapLoadingProgress.MapName);
            mapGenerator.GenerateWalls();
            mapGenerator.GenerateWater(mapConfigure.waterColor);
            _gameFactory.CreateDirectionalLight(mapConfigure.lightData);
            Environment.ApplyAmbientLighting(mapConfigure);
            Environment.ApplyFog(mapConfigure);
            _mapProjector = new VerticalMapProjector(networkManager.Client.MapProvider);
            networkManager.Client.MapUpdated += OnMapUpdated;
            networkManager.Client.MapProjector = _mapProjector;
            _uiFactory.CreateInGameUI(_gameStateMachine, networkManager, _inputService, _storageService, _avatarLoader);
        }

        private void OnMapUpdated(BlockDataWithPosition[] updatedBlocks)
        {
            _mapMeshUpdater.UpdateMesh(updatedBlocks);
            _mapProjector.UpdateProjection(updatedBlocks);
        }

        public void Exit()
        {
            _networkManager.Client.MapUpdated -= OnMapUpdated;
        }
    }
}