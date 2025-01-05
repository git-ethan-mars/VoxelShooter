using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay;
using GamePlay.Data;
using GamePlay.Entities;
using Mirror;
using Networking.Client;
using Networking.Host.Services;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;
using VoxelMap;

namespace Networking.Host
{
    public sealed partial class MirrorHost : IHost
    {
        public event Action<string, MapData> MapLoaded
        {
            add => _client.MapLoaded += value;
            remove => _client.MapLoaded -= value;
        }

        public event Action<float> MapLoadProgressed
        {
            add => _client.MapLoadProgressed += value;
            remove => _client.MapLoadProgressed -= value;
        }

        public event Action<ServerTime> GameTimeChanged
        {
            add => _client.GameTimeChanged += value;
            remove => _client.GameTimeChanged -= value;
        }

        public event Action<ServerTime> RespawnTimeChanged
        {
            add => _client.RespawnTimeChanged += value;
            remove => _client.RespawnTimeChanged -= value;
        }

        public event Action<List<ScoreData>> ScoreboardChanged
        {
            add => _client.ScoreboardChanged += value;
            remove => _client.ScoreboardChanged -= value;
        }

        public event Action<Character> CharacterSpawned;
        public event Action<Character> CharacterDespawned;

        public event Func<UniTask> GameFinished
        {
            add => _client.GameFinished += value;
            remove => _client.GameFinished -= value;
        }

        private readonly IClient _client;
        private readonly NetworkManager _networkManager;
        private readonly IEntityFactory _entityFactory;
        private readonly ICharacterFactory _characterFactory;
        private readonly IInventoryFactory _inventoryFactory;
        private readonly WorldSettings _worldSettings;
        private readonly MapProvider _mapProvider;
        private readonly HostTimer _hostTimer;
        private readonly CharacterSpawner _characterSpawner;

        private readonly Dictionary<ulong, ClientData> _clientDataById = new();
        private readonly Dictionary<NetworkConnectionToClient, ulong> _idByConnection = new();

        private readonly CancellationTokenSource _hostStoppedCts;

        public MirrorHost(IClient client, NetworkManager networkManager, IEntityFactory entityFactory,
            ICharacterFactory characterFactory, IInventoryFactory inventoryFactory,
            IMapConfigureLoader mapConfigureLoader, WorldSettings worldSettings, MapProvider mapProvider)
        {
            _client = client;
            _networkManager = networkManager;
            _entityFactory = entityFactory;
            _characterFactory = characterFactory;
            _inventoryFactory = inventoryFactory;
            _worldSettings = worldSettings;
            _mapProvider = mapProvider;
            _hostStoppedCts = new CancellationTokenSource();
            _hostTimer = new HostTimer(this, worldSettings.MaxDuration, _hostStoppedCts.Token);
            _characterSpawner =
                new CharacterSpawner(characterFactory, entityFactory, mapConfigureLoader, worldSettings);
        }

        public void Start()
        {
            _networkManager.StartHost();
            _hostTimer.Start();
            RegisterMessageHandlers();
            _client.Start();
        }

        public void Stop()
        {
            _hostStoppedCts.Cancel();
            _hostStoppedCts.Dispose();
            _networkManager.StopHost();
            UnregisterHandlers();
            _client.Stop();
        }

        public void ChangeClass(GameClass gameClass)
        {
            _client.ChangeClass(gameClass);
        }

        private bool AddPlayer(NetworkConnectionToClient connection, ulong id, string nickName)
        {
            if (!_clientDataById.ContainsKey(id))
            {
                _idByConnection[connection] = id;
                _clientDataById[id] = new ClientData(nickName);
                return true;
            }

            return _idByConnection.TryAdd(connection, id);
        }


        private void SendMap(NetworkConnectionToClient connection)
        {
            if (NetworkConnection.LocalConnectionId == connection.connectionId)
            {
                return;
            }

            connection.Send(new MapNameResponse(_worldSettings.MapName));
            var serializedMap = _mapProvider.Serialize();
            var mapMessages = MessageSplitter.SplitBytesIntoMessages(serializedMap);
            MessageSplitter.SendMessages(mapMessages, false, connection);
        }

        public void RemovePlayer(NetworkConnectionToClient connection)
        {
            _idByConnection.Remove(connection);
            NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
            NetworkServer.DestroyPlayerForConnection(connection);
        }

        private void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = GetPlayerData(connection);
            if (playerData.GameClass == GameClass.None)
            {
                playerData.GameClass = chosenClass;
                playerData.IsAlive = true;
                var character = _characterSpawner.SpawnCharacter();
                var synchronization = character.GetComponent<PlayerDataSynchronization>();
                synchronization.nickName = playerData.NickName;
                synchronization.gameClass = playerData.GameClass;
                var inventory = _inventoryFactory.CreateInventory(playerData.GameClass, _mapProvider);
                character.Initialize(inventory);
                
                if (NetworkServer.localConnection == connection)
                {
                    CharacterSpawned?.Invoke(character);    
                }
                
                NetworkServer.AddPlayerForConnection(connection, character.gameObject);
                NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
            }
            else
            {
                playerData.GameClass = chosenClass;
                if (!playerData.IsAlive)
                {
                    return;
                }

                Kill(connection);
            }
        }
        
        private bool TryGetPlayerData(NetworkConnectionToClient connection, out ClientData clientData)
        {
            if (connection is null)
            {
                clientData = null;
                return false;
            }

            if (_idByConnection.TryGetValue(connection, out var id))
            {
                return _clientDataById.TryGetValue(id, out clientData);
            }

            clientData = null;
            return false;
        }

        private ClientData GetPlayerData(NetworkConnectionToClient connection)
        {
            var id = _idByConnection[connection];
            return _clientDataById[id];
        }

        private void RespawnPlayer(NetworkConnectionToClient connection)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (!result)
            {
                return;
            }

            playerData.IsAlive = true;
            var character = _characterSpawner.SpawnCharacter();
            var synchronization = character.GetComponent<PlayerDataSynchronization>();
            synchronization.nickName = playerData.NickName;
            synchronization.gameClass = playerData.GameClass;
            var inventory = _inventoryFactory.CreateInventory(playerData.GameClass, _mapProvider);
            character.Initialize(inventory);
            
            if (NetworkServer.localConnection == connection)
            {
                CharacterSpawned?.Invoke(character);        
            }
            
            ReplacePlayer(connection, character.gameObject);
        }

        private void ReplacePlayer(NetworkConnectionToClient connection, GameObject newPlayer)
        {
            // TODO : REFACTOR
            if (connection.identity == null)
            {
                return;
            }

            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);
            DestroyPlayerAsync(oldPlayer).Forget();
        }

        private async UniTaskVoid DestroyPlayerAsync(GameObject oldPlayer)
        {
            await UniTask.Yield();
            if (oldPlayer != null)
            {
                NetworkServer.Destroy(oldPlayer);
            }
        }

        private void Kill(NetworkConnectionToClient connection)
        {
            var tombstonePosition =
                Vector3Int.FloorToInt(connection.identity.transform.position) + Map.WorldOffset;
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            var playerData = GetPlayerData(connection);
            playerData.IsAlive = false;
            playerData.Deaths += 1;
            var character = connection.identity.GetComponent<Character>();
            if (NetworkServer.localConnection == connection)
            {
                CharacterDespawned?.Invoke(character);        
            }
            
            var spectator = _characterFactory.CreateSpectator(tombstonePosition);
            ReplacePlayer(connection, spectator.gameObject);
            var respawnTimer = new RespawnTimer(connection, _worldSettings.SpawnTime,
                () => RespawnPlayer(connection));
            respawnTimer.Start();
            NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
        }

        private List<ScoreData> GetScoreData()
        {
            var scoreData = new SortedSet<ScoreData>();
            foreach (var connection in _idByConnection.Keys)
            {
                var playerData = GetPlayerData(connection);
                scoreData.Add(new ScoreData(_idByConnection[connection], playerData.NickName, playerData.Kills,
                    playerData.Deaths, playerData.GameClass));
            }

            return scoreData.ToList();
        }

        private void RegisterMessageHandlers()
        {
            NetworkServer.RegisterHandler<AuthenticationRequest>(OnRequestReceived, false);
            NetworkServer.RegisterHandler<ChangeClassRequest>(OnRequestReceived);
        }

        private void UnregisterHandlers()
        {
            NetworkServer.UnregisterHandler<AuthenticationRequest>();
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
        }
    }
}