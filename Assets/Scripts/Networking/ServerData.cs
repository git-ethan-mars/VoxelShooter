using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking.Messages;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class ServerData
    {
        public readonly Dictionary<NetworkConnectionToClient, PlayerData> DataByConnection;
        public Map Map { get; }
        public readonly List<KillData> Kills;
        private readonly IStaticDataService _staticData;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerSettings _serverSettings;

        public ServerData(ICoroutineRunner coroutineRunner, IAssetProvider assets, IStaticDataService staticDataService,
            IParticleFactory particleFactory, Map map, ServerSettings serverSettings)
        {
            _coroutineRunner = coroutineRunner;
            DataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            Kills = new List<KillData>();
            _staticData = staticDataService;
            Map = map;
            _serverSettings = serverSettings;
            _playerFactory = new PlayerFactory(assets, this, particleFactory);
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname);
            playerData.IsAlive = true;
            DataByConnection[connection] = playerData;
            ChangeClassInternal(DataByConnection[connection], chosenClass);
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
            _playerFactory.CreatePlayer(connection);
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = GetPlayerData(connection);
            if (playerData.GameClass == chosenClass) return;
            ChangeClassInternal(playerData, chosenClass);
            if (playerData.IsAlive)
            {
                playerData.IsAlive = false;
                playerData.Deaths += 1;
                _playerFactory.CreateSpectatorPlayer(connection);
                var respawnTimer = new RespawnTimer(_coroutineRunner, connection, _serverSettings.SpawnTime,
                    () => Respawn(connection, playerData));
                respawnTimer.Start();
            }
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));

        }

        private void Respawn(NetworkConnectionToClient connection, PlayerData playerData)
        {
            playerData.IsAlive = true;
            _playerFactory.RespawnPlayer(connection, playerData);
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            DataByConnection.Remove(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }


        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            if (killer is not null)
                DataByConnection[killer].Kills += 1;
            DataByConnection[victim].Deaths += 1;
            Kills.Add(new KillData(killer, victim));
            var playerData = DataByConnection[victim];
            playerData.IsAlive = false;
            _playerFactory.CreateSpectatorPlayer(victim);
            var respawnTimer = new RespawnTimer(_coroutineRunner, victim, _serverSettings.SpawnTime,
                () => Respawn(victim, playerData));
            respawnTimer.Start();
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }

        public PlayerData GetPlayerData(NetworkConnectionToClient connection)
        {
            return DataByConnection.TryGetValue(connection, out var playerData) ? playerData : null;
        }

        public int GetItemCount(NetworkConnectionToClient connection, int itemId)
        {
            var playerData = GetPlayerData(connection);
            return playerData.ItemCountById[itemId];
        }

        public void SetItemCount(NetworkConnectionToClient connection, int itemId, int value)
        {
            var playerData = GetPlayerData(connection);
            playerData.ItemCountById[itemId] = value;
        }

        private void ChangeClassInternal(PlayerData playerData, GameClass chosenClass)
        {
            playerData.GameClass = chosenClass;
            playerData.Characteristic = _staticData.GetPlayerCharacteristic(playerData.GameClass);
            playerData.Health = playerData.Characteristic.maxHealth;
            playerData.ItemCountById = new Dictionary<int, int>();
            playerData.ItemsId = _staticData.GetInventory(playerData.GameClass).Select(item => item.id).ToList();
            playerData.RangeWeaponsById = new Dictionary<int, RangeWeaponData>();
            playerData.MeleeWeaponsById = new Dictionary<int, MeleeWeaponData>();
            playerData.ItemCountById = new Dictionary<int, int>();
            foreach (var itemId in playerData.ItemsId)
            {
                var item = _staticData.GetItem(itemId);
                if (item.itemType == ItemType.RangeWeapon)
                {
                    playerData.RangeWeaponsById[itemId] = new RangeWeaponData((RangeWeaponItem) item);
                }

                if (item.itemType == ItemType.MeleeWeapon)
                {
                    playerData.MeleeWeaponsById[itemId] = new MeleeWeaponData((MeleeWeaponItem) item);
                }

                if (item.itemType == ItemType.Tnt)
                {
                    playerData.ItemCountById[itemId] = ((TntItem) item).count;
                    continue;
                }
                
                if (item.itemType == ItemType.Grenade)
                {
                    playerData.ItemCountById[itemId] = ((GrenadeItem) item).count;
                    continue;
                }

                if (item.itemType == ItemType.Block)
                {
                    playerData.ItemCountById[itemId] = ((BlockItem) item).count;
                    continue;
                }
                
                if (item.itemType == ItemType.RocketLauncher)
                {
                    playerData.ItemCountById[itemId] = ((RocketLauncherItem) item).count;
                    continue;
                }

                playerData.ItemCountById[itemId] = 1;
            }
        }

        private List<ScoreData> GetScoreData()
        {
            var scoreData = new SortedSet<ScoreData>();
            foreach (var playerData in DataByConnection.Values)
            {
                scoreData.Add(new ScoreData(playerData.SteamID, playerData.NickName, playerData.Kills,
                    playerData.Deaths, playerData.GameClass));
            }

            return scoreData.ToList();
        }
    }
}