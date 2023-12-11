using System;
using System.Collections.Generic;
using Data;
using Generators;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using MapLogic;
using Mirror;
using Networking.ClientServices;
using Networking.MessageHandlers.ResponseHandler;
using PlayerLogic;
using PlayerLogic.Spectator;
using UI.SettingsMenu;
using UnityEngine;
using Environment = MapLogic.Environment;

namespace Networking
{
    public class Client : IClient
    {
        public event Action GameFinished;
        public event Action MapDownloaded;

        public event Action<float> MapLoadProgressed
        {
            add => _downloadMapHandler.MapLoadProgressed += value;
            remove => _downloadMapHandler.MapLoadProgressed -= value;
        }

        public event Action<ServerTime> GameTimeChanged
        {
            add => _gameTimeHandler.GameTimeChanged += value;
            remove => _gameTimeHandler.GameTimeChanged -= value;
        }

        public event Action<ServerTime> RespawnTimeChanged
        {
            add => _respawnTimeHandler.RespawnTimeChanged += value;
            remove => _respawnTimeHandler.RespawnTimeChanged -= value;
        }

        public event Action<List<ScoreData>> ScoreboardChanged
        {
            add => _scoreboardHandler.ScoreboardChanged += value;
            remove => _scoreboardHandler.ScoreboardChanged -= value;
        }

        public event Action<Player> PlayerCreated
        {
            add => _playerConfigureHandler.PlayerCreated += value;
            remove => _playerConfigureHandler.PlayerCreated += value;
        }

        public FallMeshGenerator FallMeshGenerator { get; }
        public ClientData Data { get; }
        public MapGenerator MapGenerator { get; private set; }


        public MapProvider MapProvider
        {
            get => _mapProvider;
            set
            {
                _mapProvider = value;
                MapDownloaded?.Invoke();
            }
        }

        private MapProvider _mapProvider;
        private readonly CustomNetworkManager _customNetworkManager;
        private readonly IGameFactory _gameFactory;
        private readonly IMeshFactory _meshFactory;
        private readonly IStorageService _storageService;
        private readonly GameStateMachine _stateMachine;
        private readonly MapNameHandler _mapNameHandler;
        private readonly DownloadMapHandler _downloadMapHandler;
        private readonly UpdateMapHandler _updateMapHandler;
        private readonly FallBlockHandler _fallBlockHandler;
        private readonly GameTimeHandler _gameTimeHandler;
        private readonly RespawnTimeHandler _respawnTimeHandler;
        private readonly ScoreboardHandler _scoreboardHandler;
        private readonly HealthHandler _healthHandler;
        private readonly ChangeItemModelHandler _changeItemModelHandler;
        private readonly PlayerConfigureHandler _playerConfigureHandler;
        private readonly SpectatorConfigureHandler _spectatorConfigureHandler;
        private readonly NickNameHandler _nickNameHandler;
        private readonly PlayerSoundHandler _playerSoundHandler;
        private readonly StartContinuousSoundHandler _startContinuousSoundHandler;
        private readonly StopContinuousSoundHandler _stopContinuousSoundHandler;
        private readonly SurroundingSoundHandler _surroundingSoundHandler;

        public Client(GameStateMachine stateMachine, CustomNetworkManager customNetworkManager,
            IInputService inputService,
            IStorageService storageService,
            IGameFactory gameFactory,
            IMeshFactory meshFactory,
            IStaticDataService staticData,
            IParticleFactory particleFactory, IUIFactory uiFactory)
        {
            _stateMachine = stateMachine;
            _customNetworkManager = customNetworkManager;
            _gameFactory = gameFactory;
            _meshFactory = meshFactory;
            _storageService = storageService;
            var fallingMeshParticlePool = new FallingMeshParticlePool(gameFactory, particleFactory);
            FallMeshGenerator = new FallMeshGenerator(meshFactory, fallingMeshParticlePool);
            Data = new ClientData();
            _mapNameHandler = new MapNameHandler(this);
            _downloadMapHandler = new DownloadMapHandler(this, staticData);
            _updateMapHandler = new UpdateMapHandler(this);
            _fallBlockHandler = new FallBlockHandler(this);
            _gameTimeHandler = new GameTimeHandler();
            _respawnTimeHandler = new RespawnTimeHandler();
            _scoreboardHandler = new ScoreboardHandler();
            _healthHandler = new HealthHandler();
            _changeItemModelHandler = new ChangeItemModelHandler(meshFactory, staticData);
            _playerConfigureHandler =
                new PlayerConfigureHandler(this, particleFactory, uiFactory, meshFactory, inputService, storageService,
                    staticData);
            _spectatorConfigureHandler = new SpectatorConfigureHandler(inputService, storageService);
            _nickNameHandler = new NickNameHandler();
            var audioPool = new AudioPool(gameFactory);
            _playerSoundHandler = new PlayerSoundHandler(staticData, storageService, customNetworkManager, audioPool);
            _startContinuousSoundHandler = new StartContinuousSoundHandler(staticData);
            _stopContinuousSoundHandler = new StopContinuousSoundHandler();
            _surroundingSoundHandler =
                new SurroundingSoundHandler(staticData, storageService, customNetworkManager, audioPool);
        }

        public void Start()
        {
            MapDownloaded += OnMapDownloaded;
            AudioListener.volume = _storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey).MasterVolume;
            _storageService.DataSaved += OnDataSaved;
            RegisterHandlers();
            Data.State = ClientState.Connecting;
        }

        public void Stop()
        {
            MapDownloaded -= OnMapDownloaded;
            _storageService.DataSaved -= OnDataSaved;
            UnregisterHandlers();
            Data.State = ClientState.NotConnected;
            GameFinished?.Invoke();
        }

        private void RegisterHandlers()
        {
            _mapNameHandler.Register();
            _downloadMapHandler.Register();
            _updateMapHandler.Register();
            _fallBlockHandler.Register();
            _gameTimeHandler.Register();
            _respawnTimeHandler.Register();
            _scoreboardHandler.Register();
            _healthHandler.Register();
            _changeItemModelHandler.Register();
            _playerConfigureHandler.Register();
            _spectatorConfigureHandler.Register();
            _nickNameHandler.Register();
            _playerSoundHandler.Register();
            _startContinuousSoundHandler.Register();
            _stopContinuousSoundHandler.Register();
            _surroundingSoundHandler.Register();
        }

        private void UnregisterHandlers()
        {
            _mapNameHandler.Unregister();
            _downloadMapHandler.Unregister();
            _updateMapHandler.Unregister();
            _fallBlockHandler.Unregister();
            _gameTimeHandler.Unregister();
            _respawnTimeHandler.Unregister();
            _scoreboardHandler.Unregister();
            _healthHandler.Unregister();
            _changeItemModelHandler.Unregister();
            _playerConfigureHandler.Unregister();
            _spectatorConfigureHandler.Unregister();
            _nickNameHandler.Unregister();
            _playerSoundHandler.Unregister();
            _startContinuousSoundHandler.Unregister();
            _stopContinuousSoundHandler.Unregister();
            _surroundingSoundHandler.Unregister();
        }

        private void OnMapDownloaded()
        {
            Data.State = ClientState.Connected;
            MapGenerator = new MapGenerator(_mapProvider, _gameFactory, _meshFactory);
            MapGenerator.GenerateMap();
            MapGenerator.GenerateWalls();
            MapGenerator.GenerateLight();
            Environment.ApplyAmbientLighting(_mapProvider.SceneData);
            Environment.ApplyFog(_mapProvider.SceneData);
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(_customNetworkManager);
        }

        private void OnDataSaved(ISettingsData data)
        {
            var identity = NetworkClient.connection.identity;
            if (data is VolumeSettingsData volumeSettings)
            {
                _playerSoundHandler.SoundMultiplier = volumeSettings.SoundVolume;
                _surroundingSoundHandler.SoundMultiplier = volumeSettings.SoundVolume;
                if (identity != null && identity.TryGetComponent<Player>(out var player))
                {
                    player.Audio.ChangeSoundMultiplier(volumeSettings.SoundVolume);
                }
            }

            if (data is MouseSettingsData mouseSettings)
            {
                if (identity == null)
                {
                    return;
                }

                if (identity.TryGetComponent<Player>(out var player))
                {
                    player.Rotation.ChangeMouseSettings(mouseSettings);
                }

                if (identity.TryGetComponent<SpectatorPlayer>(out var spectator))
                {
                    spectator.Rotation.ChangeMouseSettings(mouseSettings);
                }
            }
        }
    }
}