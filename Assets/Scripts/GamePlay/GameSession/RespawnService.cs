using System;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace GamePlay
{
    public class RespawnService
    {
        private readonly IEntityFactory _entityFactory;
        private readonly SpawnPointService _spawnPointService;

        private TimeSpan _respawnTime;

        public RespawnService(IEntityFactory entityFactory, SpawnPointService spawnPointService)
        {
            _entityFactory = entityFactory;
            _spawnPointService = spawnPointService;
        }

        public void SetRespawnTime(TimeSpan respawnTime) => _respawnTime = respawnTime;

        public void OnUpdate(IReadOnlyDictionary<NetworkConnectionToClient, DeathMatchPlayerSession> sessions, float deltaTime)
        {
            foreach (var session in sessions.Values)
            {
                if (session.IsAlive || session.Data.GameClass == GameClass.None)
                    continue;

                session.RespawnTime -= TimeSpan.FromSeconds(deltaTime);

                if (session.RespawnTime <= TimeSpan.Zero)
                {
                    Respawn(session);
                }
            }
        }

        public void Respawn(DeathMatchPlayerSession session)
        {
            Vector3 position = _spawnPointService.GetSpawnPoint();
            Character character = _entityFactory.CreateCharacter(
                position, session.Data.GameClass, session.Data.NickName);

            NetworkServer.ReplacePlayerForConnection(
                session.Connection, character.gameObject, ReplacePlayerOptions.Destroy);

            session.IsAlive = true;
            session.RespawnTime = TimeSpan.Zero;
        }

        public void Kill(DeathMatchPlayerSession session)
        {
            if (!session.IsAlive)
                return;

            Vector3 position = session.Connection.identity.transform.position;
            Tombstone tombstone = _entityFactory.CreateTombstone(position, session.Connection);
            tombstone.ExplodeWithDelay(_respawnTime - TimeSpan.FromSeconds(1)).Forget();

            Spectator spectator = _entityFactory.CreateSpectator(position);
            NetworkServer.ReplacePlayerForConnection(
                session.Connection, spectator.gameObject, ReplacePlayerOptions.Destroy);

            session.Connection.Send(new CharacterDiedResponse());

            session.IsAlive = false;
            session.RespawnTime = _respawnTime;
        }
    }
}