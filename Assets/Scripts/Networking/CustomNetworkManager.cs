using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using MapLogic;
using Mirror;
using Networking.Messages;
using Networking.Synchronization;
using Networking.Transport;
using PlayerLogic;
using Steamworks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Networking
{
    public class CustomNetworkManager : NetworkManager
    {
        public event Action<Map> MapLoaded;
        public ServerData ServerData { get; private set; }
        public IEntityFactory EntityFactory { get; set; }
        
        private Map _map;
        private IStaticDataService _staticData;
        private MapMessageHandler _mapMessageHandler;

        public void Construct(IStaticDataService staticData, MapMessageHandler mapMessageHandler)
        {
            _staticData = staticData;
            _mapMessageHandler = mapMessageHandler;
        }

        public override void OnStartServer()
        {
            ServerData = new ServerData(_staticData);
            NetworkServer.RegisterHandler<CharacterMessage>(OnChooseClass);
            _map = MapReader.ReadFromFile("Crossroads.rch");
            MapLoaded?.Invoke(_map);
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<DownloadMapMessage>(_mapMessageHandler.OnMapDownloadMessage);
            NetworkClient.RegisterHandler<UpdateMapMessage>(_mapMessageHandler.OnMapUpdateMessage);
            NetworkClient.RegisterHandler<HealthMessage>(OnHealthChange);
        }


        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            if (NetworkClient.connection.connectionId == conn.connectionId) return;
            DownloadMapMessage[] mapMessages = SplitMap(100000);
            StartCoroutine(SendMap(conn, mapMessages));
        }

        private IEnumerator SendMap(NetworkConnectionToClient conn, DownloadMapMessage[] messages)
        {
            for (var i = 0; i < messages.Length; i++)
            {
                conn.Send(messages[i]);
                yield return new WaitForSeconds(1);
                Debug.Log($"{(int) ((float) i / messages.Length * 100)}%");
            }
        }


        public override void OnStopServer()
        {
        }

        public void ChangeClass(GameClass gameClass)
        {
            CharacterMessage characterMessage = new CharacterMessage()
                {GameClass = gameClass, NickName = GetComponent<SteamManager>() is not null ? SteamFriends.GetPersonaName() : ""};
            NetworkClient.Send(characterMessage);
        }


        private void OnChooseClass(NetworkConnectionToClient conn, CharacterMessage message)
        {
            var player = EntityFactory.CreatePlayer(conn, message.GameClass, message.NickName);
            NetworkServer.AddPlayerForConnection(conn, player);
            ServerData.AddPlayer(conn, message.GameClass, message.NickName);
        }


        private void OnHealthChange(HealthMessage healthMessage)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<HealthSystem>()
                .UpdateHealth(healthMessage.CurrentHealth, healthMessage.MaxHealth);
        }
        


        private DownloadMapMessage[] SplitMap(int packageSize)
        {
            var memoryStream = new MemoryStream();
            MapWriter.WriteMap(_map, memoryStream);
            var bytes = memoryStream.ToArray();
            var messages = new List<DownloadMapMessage>();

            for (var i = 0; i < bytes.Length; i += packageSize)
            {
                if (bytes.Length <= i + packageSize)
                {
                    messages.Add(new DownloadMapMessage(bytes[i..bytes.Length], true));
                }
                else
                {
                    messages.Add(new DownloadMapMessage(bytes[i..(i + packageSize)], false));
                }
            }

            return messages.ToArray();
        }
    }
}