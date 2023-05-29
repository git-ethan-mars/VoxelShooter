using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages;
using Networking.Synchronization;
using UnityEngine;

namespace Networking
{
    public class ServerMessageHandlers
    {
        private readonly IEntityFactory _entityFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerData _serverData;
        private readonly IStaticDataService _staticData;
        private readonly IParticleFactory _particleFactory;
        private readonly IExplosionArea _sphereExplosionArea;

        public ServerMessageHandlers(IEntityFactory entityFactory, ICoroutineRunner coroutineRunner,
            ServerData serverData, IStaticDataService staticData, IParticleFactory particleFactory)
        {
            _serverData = serverData;
            _entityFactory = entityFactory;
            _coroutineRunner = coroutineRunner;
            _staticData = staticData;
            _particleFactory = particleFactory;
            _sphereExplosionArea = new SphereExplosionArea(serverData);
        }

        public void RegisterHandlers()
        {
            NetworkServer.RegisterHandler<ChangeClassRequest>(OnChangeClass);
            NetworkServer.RegisterHandler<TntSpawnRequest>(OnTntSpawn);
            NetworkServer.RegisterHandler<GrenadeSpawnRequest>(OnGrenadeSpawn);
            NetworkServer.RegisterHandler<RocketLauncherSpawnRequest>(OnRocketLauncherSpawn);
            NetworkServer.RegisterHandler<AddBlocksRequest>(OnAddBlocks);
            NetworkServer.RegisterHandler<ChangeSlotRequest>(OnChangeSlot);
        }

        public void RemoveHandlers()
        {
            NetworkServer.UnregisterHandler<ChangeClassRequest>();
            NetworkServer.UnregisterHandler<TntSpawnRequest>();
            NetworkServer.UnregisterHandler<GrenadeSpawnRequest>();
            NetworkServer.UnregisterHandler<RocketLauncherSpawnRequest>();
            NetworkServer.UnregisterHandler<AddBlocksRequest>();
            NetworkServer.UnregisterHandler<ChangeSlotRequest>();
        }

        private void OnChangeClass(NetworkConnectionToClient connection, ChangeClassRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out _);
            if (!result)
            {
                _serverData.AddPlayer(connection, message.GameClass, message.SteamID, message.Nickname);
            }
            else
            {
                _serverData.ChangeClass(connection, message.GameClass);
            }
        }

        private void OnAddBlocks(NetworkConnectionToClient connection, AddBlocksRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var blockAmount = playerData.ItemCountById[message.ItemId];
            var validPositions = new List<Vector3Int>();
            var validBlockData = new List<BlockData>();
            var blocksUsed = Math.Min(blockAmount, message.GlobalPositions.Length);
            for (var i = 0; i < blocksUsed; i++)
            {
                foreach (var otherConnection in _serverData.GetConnections())
                {
                    result = _serverData.TryGetPlayerData(otherConnection, out var otherPlayer);
                    if (!result || !otherPlayer.IsAlive) continue;
                    var playerPosition = otherConnection.identity.gameObject.transform.position;
                    var blockPosition = message.GlobalPositions[i];
                    if (playerPosition.x > blockPosition.x
                        && playerPosition.x < blockPosition.x + 1
                        && playerPosition.z > blockPosition.z
                        && playerPosition.z < blockPosition.z + 1
                        && playerPosition.y > blockPosition.y - 2
                        && playerPosition.y < blockPosition.y + 2)
                        return;
                }

                if (!_serverData.Map.IsValidPosition(message.GlobalPositions[i])) return;
                var currentBlock = _serverData.Map.GetBlockByGlobalPosition(message.GlobalPositions[i]);
                if (currentBlock.Equals(message.Blocks[i])) return;
                validPositions.Add(message.GlobalPositions[i]);
                validBlockData.Add(message.Blocks[i]);
            }

            playerData.ItemCountById[message.ItemId] = blockAmount - blocksUsed;
            NetworkServer.SendToAll(new UpdateMapMessage(validPositions.ToArray(), validBlockData.ToArray()));
            connection.Send(new ItemUseResult(message.ItemId, blockAmount - blocksUsed));
        }

        private void OnRocketLauncherSpawn(NetworkConnectionToClient connection, RocketLauncherSpawnRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var rocketCount = playerData.ItemCountById[message.ItemId];
            if (rocketCount <= 0)
                return;
            playerData.ItemCountById[message.ItemId] = rocketCount - 1;
            connection.Send(new ItemUseResult(message.ItemId, rocketCount - 1));
            var rocketData = (RocketLauncherItem) _staticData.GetItem(message.ItemId);
            var direction = message.Ray.direction;
            var rocket = _entityFactory.CreateRocket(message.Ray.origin + direction * 2,
                Quaternion.LookRotation(direction), _serverData, _particleFactory, rocketData, connection);
            rocket.GetComponent<Rigidbody>().velocity = direction * rocketData.speed;
        }

        private void OnGrenadeSpawn(NetworkConnectionToClient connection, GrenadeSpawnRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var grenadeCount = playerData.ItemCountById[message.ItemId];
            if (grenadeCount <= 0)
                return;
            playerData.ItemCountById[message.ItemId] = grenadeCount - 1;
            connection.Send(new ItemUseResult(message.ItemId, grenadeCount - 1));
            var grenade = _entityFactory.CreateGrenade(message.Ray.origin, Quaternion.identity);
            grenade.GetComponent<Rigidbody>().AddForce(message.Ray.direction * message.ThrowForce);
            var grenadeData = (GrenadeItem) _staticData.GetItem(message.ItemId);
            _coroutineRunner.StartCoroutine(ExplodeGrenade(grenade, grenadeData.delayInSeconds, grenadeData.radius,
                grenadeData.damage, grenadeData.particlesSpeed, grenadeData.particlesCount, connection));
        }

        private IEnumerator ExplodeGrenade(GameObject grenade, float delayInSeconds, int radius, int damage,
            int particlesSpeed, int particlesCount, NetworkConnectionToClient connection)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (!grenade) yield break;

            var grenadePosition = new Vector3Int((int) grenade.transform.position.x,
                (int) grenade.transform.position.y, (int) grenade.transform.position.z);

            var blockPositions = _sphereExplosionArea.GetExplodedBlocks(radius, grenadePosition);

            foreach (var position in blockPositions)
                _serverData.Map.SetBlockByGlobalPosition(position, new BlockData());
            _particleFactory.CreateRchParticle(grenadePosition, particlesSpeed, particlesCount);

            NetworkServer.SendToAll(new UpdateMapMessage(blockPositions.ToArray(),
                new BlockData[blockPositions.Count]));
            NetworkServer.Destroy(grenade);

            Collider[] hitColliders = Physics.OverlapSphere(grenadePosition, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    var playerPosition = hitCollider.transform.position;
                    var distance = Math.Sqrt(
                        (grenadePosition.x - playerPosition.x) * (grenadePosition.x - playerPosition.x) +
                        (grenadePosition.y - playerPosition.y) * (grenadePosition.y - playerPosition.y) +
                        (grenadePosition.z - playerPosition.z) * (grenadePosition.z - playerPosition.z));
                    var currentDamage = (int) (damage - damage * (distance / radius));
                    if (distance >= radius)
                    {
                        distance = radius;
                    }
                    var direction = playerPosition - grenadePosition;
                    hitCollider.GetComponent<CharacterController>().Move(direction * particlesSpeed / 3);
                    var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                    receiver.identity.GetComponent<HealthSynchronization>().Damage(connection, receiver, currentDamage);
                }
            }
        }

        private void OnTntSpawn(NetworkConnectionToClient connection, TntSpawnRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            var tntCount = playerData.ItemCountById[message.ItemId];
            if (tntCount <= 0)
                return;
            var tnt = _entityFactory.CreateTnt(message.Position, message.Rotation);
            var tntData = (TntItem) _staticData.GetItem(message.ItemId);
            playerData.ItemCountById[message.ItemId] = tntCount - 1;
            _coroutineRunner.StartCoroutine(ExplodeWithDelay(Vector3Int.FloorToInt(message.ExplosionCenter), tnt,
                tntData.delayInSeconds,
                tntData.radius, connection, tntData.damage, tntData.particlesSpeed, tntData.particlesCount));
            connection.Send(new ItemUseResult(message.ItemId, tntCount - 1));
        }

        private IEnumerator ExplodeWithDelay(Vector3Int explosionCenter, GameObject tnt, float delayInSeconds,
            int radius, NetworkConnectionToClient connection, int damage, int particlesSpeed, int particlesCount)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (!tnt) yield break;
            var explodedTnt = new List<GameObject>();
            ExplodeImmediately(explosionCenter, tnt, radius, explodedTnt, connection, damage, particlesSpeed,
                particlesCount);
        }

        private void ExplodeImmediately(Vector3Int explosionCenter, GameObject tnt, int radius,
            List<GameObject> explodedTnt, NetworkConnectionToClient connection, int damage, int particlesSpeed,
            int particlesCount)
        {
            var blockPositions = _sphereExplosionArea.GetExplodedBlocks(radius, explosionCenter);

            foreach (var position in blockPositions)
                _serverData.Map.SetBlockByGlobalPosition(position, new BlockData());
            _particleFactory.CreateRchParticle(explosionCenter, particlesSpeed, particlesCount);

            NetworkServer.SendToAll(new UpdateMapMessage(blockPositions.ToArray(),
                new BlockData[blockPositions.Count]));
            NetworkServer.Destroy(tnt);
            explodedTnt.Add(tnt);

            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("TNT") && explodedTnt.All(x => x.gameObject != hitCollider.gameObject))
                {
                    ExplodeImmediately(Vector3Int.FloorToInt(hitCollider.gameObject.transform.position),
                        hitCollider.gameObject, radius, explodedTnt, connection, damage, particlesSpeed,
                        particlesCount);
                }

                if (hitCollider.CompareTag("Player"))
                {
                    var playerPosition = hitCollider.transform.position;
                    var distance = Math.Sqrt(
                        (explosionCenter.x - playerPosition.x) * (explosionCenter.x - playerPosition.x) +
                        (explosionCenter.y - playerPosition.y) * (explosionCenter.y - playerPosition.y) +
                        (explosionCenter.z - playerPosition.z) * (explosionCenter.z - playerPosition.z));
                    if (distance >= radius)
                    {
                        distance = radius;
                    }
                    var currentDamage = (int) (damage - damage * (distance / radius));
                    var direction = playerPosition - explosionCenter;
                    hitCollider.GetComponent<CharacterController>().Move(direction * particlesSpeed / 3);
                    var receiver = hitCollider.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                    if (receiver.identity.TryGetComponent<HealthSynchronization>(out var health))
                        health.Damage(connection, receiver, currentDamage);
                }
            }
        }

        private void OnChangeSlot(NetworkConnectionToClient connection, ChangeSlotRequest message)
        {
            var result = _serverData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive) return;
            connection.identity.GetComponent<PlayerLogic.Inventory>().currentSlotId = message.Index;
            connection.Send(new ChangeSlotResult(message.Index));
        }
    }
}