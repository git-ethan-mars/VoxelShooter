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
        private ChunkMeshUpdater _chunkMeshUpdater;
        private VerticalMapProjector _mapProjector;

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
            var mapGenerator = new MapGenerator(networkManager.Client.MapProvider, _gameFactory, _meshFactory);
            var chunkMeshes = mapGenerator.GenerateMap(_gameFactory.CreateGameObjectContainer(ChunkContainerName));
            _chunkMeshUpdater = new ChunkMeshUpdater(networkManager.Client, chunkMeshes);
            var mapConfigure = _staticData.GetMapConfigure(networkManager.Client.MapName);
            mapGenerator.GenerateWalls();
            mapGenerator.GenerateWater(mapConfigure.waterColor);
            _gameFactory.CreateDirectionalLight(mapConfigure.lightData);
            Environment.ApplyAmbientLighting(mapConfigure);
            Environment.ApplyFog(mapConfigure);
            _mapProjector = new VerticalMapProjector(networkManager.Client);
            networkManager.Client.MapProjector = _mapProjector;
            _uiFactory.CreateInGameUI(_gameStateMachine, networkManager, _inputService, _storageService, _avatarLoader);
        }

        public void Exit()
        {
            _chunkMeshUpdater.Dispose();
            _mapProjector.Dispose();
        }
    }
}