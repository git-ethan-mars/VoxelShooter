using Generators;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.Storage;
using MapLogic;
using Networking;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<CustomNetworkManager>
    {
        private const string ChunkContainerName = "ChunkContainer";

        private readonly GameStateMachine _gameStateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IUIFactory _uiFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IAvatarLoader _avatarLoader;
        private ChunkMeshUpdater _chunkMeshUpdater;
        private VerticalMapProjector _mapProjector;

        public GameLoopState(GameStateMachine gameStateMachine, IGameFactory gameFactory, IUIFactory uiFactory,
            IMeshFactory meshFactory, IInputService inputService,
            IStorageService storageService,
            IAvatarLoader avatarLoader)
        {
            _gameStateMachine = gameStateMachine;
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
            mapGenerator.GenerateWalls();
            mapGenerator.GenerateWater();
            mapGenerator.GenerateLight();
            Environment.ApplyAmbientLighting(networkManager.Client.MapProvider.SceneData);
            Environment.ApplyFog(networkManager.Client.MapProvider.SceneData);
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