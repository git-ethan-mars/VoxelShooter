﻿using System;
using System.Collections.Generic;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
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
using Object = UnityEngine.Object;

namespace Networking
{
    public class Client : IClient
    {
        public event Action GameFinished;
        public event Action MapDownloaded;

        public event Action<BlockDataWithPosition[]> MapUpdated
        {
            add => _updateMapHandler.MapUpdated += value;
            remove => _updateMapHandler.MapUpdated -= value;
        }

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

        public MapProvider MapProvider
        {
            get => _mapProvider;
            set
            {
                _mapProvider = value;
                MapDownloaded?.Invoke();
            }
        }

        public VerticalMapProjector MapProjector { get; set; }
        public string MapName { get; set; }

        private readonly GameStateMachine _stateMachine;
        private readonly CustomNetworkManager _networkManager;
        private readonly IAssetProvider _assets;
        private readonly IStorageService _storageService;
        private readonly IPlayerFactory _playerFactory;
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
        private MapProvider _mapProvider;


        public Client(GameStateMachine stateMachine, CustomNetworkManager networkManager)
        {
            _stateMachine = stateMachine;
            _networkManager = networkManager;
            _assets = networkManager.Assets;
            _storageService = networkManager.StorageService;
            _playerFactory = networkManager.PlayerFactory;
            _mapNameHandler = new MapNameHandler(this);
            _downloadMapHandler = new DownloadMapHandler(this, networkManager.StaticData);
            _updateMapHandler = new UpdateMapHandler(this);
            _fallBlockHandler = new FallBlockHandler(networkManager);
            _gameTimeHandler = new GameTimeHandler();
            _respawnTimeHandler = new RespawnTimeHandler();
            _scoreboardHandler = new ScoreboardHandler();
            _healthHandler = new HealthHandler();
            _changeItemModelHandler =
                new ChangeItemModelHandler(networkManager.MeshFactory, networkManager.StaticData);
            _playerConfigureHandler =
                new PlayerConfigureHandler(this, networkManager.ParticleFactory);
            _spectatorConfigureHandler = new SpectatorConfigureHandler(networkManager.InputService,
                networkManager.StorageService);
            _nickNameHandler = new NickNameHandler();
            var audioPool = new AudioPool(networkManager.GameFactory);
            _playerSoundHandler = new PlayerSoundHandler(networkManager, audioPool);
            _startContinuousSoundHandler = new StartContinuousSoundHandler(networkManager.StaticData);
            _stopContinuousSoundHandler = new StopContinuousSoundHandler();
            _surroundingSoundHandler =
                new SurroundingSoundHandler(networkManager, audioPool);
        }

        public void Start()
        {
            MapDownloaded += OnMapDownloaded;
            _storageService.DataSaved += OnDataSaved;
            RegisterHandlers();
            NetworkClient.RegisterPrefab(_assets.Load<GameObject>(PlayerPath.MainPlayerPath), SpawnPlayerHandler,
                UnSpawnPlayerHandler);
        }


        public void Stop()
        {
            MapDownloaded -= OnMapDownloaded;
            MapProjector.Dispose();
            _storageService.DataSaved -= OnDataSaved;
            UnregisterHandlers();
            NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(PlayerPath.MainPlayerPath));
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
            MapProjector = new VerticalMapProjector(this);
            _stateMachine.Enter<GameLoopState, CustomNetworkManager>(_networkManager);
        }

        private void OnDataSaved(ISettingsData data)
        {
            var identity = NetworkClient.connection.identity;
            if (data is VolumeSettingsData volumeSettings)
            {
                _playerSoundHandler.SoundMultiplier = volumeSettings.SoundVolume;
                _surroundingSoundHandler.SoundMultiplier = volumeSettings.SoundVolume;
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

        private GameObject SpawnPlayerHandler(SpawnMessage msg)
        {
            return _playerFactory.CreatePlayer(msg.position);
        }

        private void UnSpawnPlayerHandler(GameObject spawned)
        {
            Object.Destroy(spawned);
        }
    }
}