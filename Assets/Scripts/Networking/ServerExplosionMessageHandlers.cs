using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace Networking
{
    public partial class ServerMessageHandlers
    {
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

            _explosionManager.DestroyExplosiveWithBlocks(grenadePosition, grenade, radius, particlesSpeed, particlesCount);
            Collider[] hitColliders = Physics.OverlapSphere(grenadePosition, radius);
            foreach (var hitCollider in hitColliders)
                _explosionManager.DamagePlayer(hitCollider, grenadePosition, radius, damage, particlesSpeed, connection);
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
            _coroutineRunner.StartCoroutine(ExplodeTntWithDelay(Vector3Int.FloorToInt(message.ExplosionCenter), tnt,
                tntData.delayInSeconds,
                tntData.radius, connection, tntData.damage, tntData.particlesSpeed, tntData.particlesCount));
            connection.Send(new ItemUseResult(message.ItemId, tntCount - 1));
        }

        private IEnumerator ExplodeTntWithDelay(Vector3Int explosionCenter, GameObject tnt, float delayInSeconds,
            int radius, NetworkConnectionToClient connection, int damage, int particlesSpeed, int particlesCount)
        {
            yield return new WaitForSeconds(delayInSeconds);
            if (!tnt) yield break;
            var explodedTnt = new List<GameObject>();
            ExplodeTntImmediately(explosionCenter, tnt, radius, explodedTnt, connection, damage, particlesSpeed,
                particlesCount);
        }

        private void ExplodeTntImmediately(Vector3Int explosionCenter, GameObject tnt, int radius,
            List<GameObject> explodedTnt, NetworkConnectionToClient connection, int damage, int particlesSpeed,
            int particlesCount)
        {
            explodedTnt.Add(tnt);
            
            _explosionManager.DestroyExplosiveWithBlocks(explosionCenter, tnt, radius, particlesSpeed, particlesCount);
            Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, radius);
            foreach (var hitCollider in hitColliders)
            {
                _explosionManager.DetonateExplosive(() => ExplodeTntImmediately(Vector3Int.FloorToInt(hitCollider.gameObject.transform.position),
                    hitCollider.gameObject, radius, explodedTnt, connection, damage, particlesSpeed,
                    particlesCount), hitCollider, explodedTnt, "TNT");
                _explosionManager.DamagePlayer(hitCollider, explosionCenter, radius, damage, particlesSpeed, connection);
            }
        }
    }
}