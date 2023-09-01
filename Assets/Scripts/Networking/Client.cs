using System;
using System.Collections.Generic;
using Data;
using Generators;
using Infrastructure.Factory;
using MapLogic;

namespace Networking
{
    public class Client
    {
        public event Action MapDownloaded
        {
            add => _downloadMapHandler.MapDownloaded += value;
            remove => _downloadMapHandler.MapDownloaded -= value;
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

        public readonly ClientData Data;
        public readonly IGameFactory GameFactory;
        public readonly IMeshFactory MeshFactory;
        public readonly FallMeshGenerator FallMeshGenerator;
        public IMapProvider MapProvider { get; set; }
        public MapGenerator MapGenerator { get; set; }

        private readonly DownloadMapHandler _downloadMapHandler;
        private readonly UpdateMapHandler _updateMapHandler;
        private readonly FallBlockHandler _fallBlockHandler;
        private readonly GameTimeHandler _gameTimeHandler;
        private readonly RespawnTimeHandler _respawnTimeHandler;
        private readonly ScoreboardHandler _scoreboardHandler;

        public Client(IGameFactory gameFactory, IMeshFactory meshFactory)
        {
            GameFactory = gameFactory;
            MeshFactory = meshFactory;
            FallMeshGenerator = new FallMeshGenerator(meshFactory);
            Data = new ClientData();
            _downloadMapHandler = new DownloadMapHandler(this);
            _updateMapHandler = new UpdateMapHandler(this);
            _fallBlockHandler = new FallBlockHandler(this);
            _gameTimeHandler = new GameTimeHandler();
            _respawnTimeHandler = new RespawnTimeHandler();
            _scoreboardHandler = new ScoreboardHandler();
        }

        public void RegisterHandlers()
        {
            _downloadMapHandler.Register();
            _updateMapHandler.Register();
            _fallBlockHandler.Register();
            _gameTimeHandler.Register();
            _respawnTimeHandler.Register();
            _scoreboardHandler.Register();
        }

        public void UnregisterHandlers()
        {
            _downloadMapHandler.Unregister();
            _updateMapHandler.Unregister();
            _fallBlockHandler.Unregister();
            _gameTimeHandler.Unregister();
            _respawnTimeHandler.Unregister();
            _scoreboardHandler.Unregister();
        }
    }
}