using Mirror;
using Networking;
using UnityEngine;

namespace Explosions
{
    public class ExplosionBehaviour
    {
        private readonly IServer _server;
        private readonly NetworkConnectionToClient _owner;
        private readonly int _radius;
        private readonly int _damage;
        private readonly BlockDestructionBehaviour _blockDestructionBehaviour;
        private Vector3 _explosionCenter;

        public ExplosionBehaviour(IServer server, NetworkConnectionToClient owner, int radius, int damage)
        {
            _server = server;
            _owner = owner;
            _radius = radius;
            _damage = damage;
            _blockDestructionBehaviour =
                new BlockDestructionBehaviour(server, new SphereBlockArea(server.MapProvider, radius));
        }

        public void Explode(Vector3 explosionCenter)
        {
            _explosionCenter = explosionCenter;
            _blockDestructionBehaviour.DamageBlocks(Vector3Int.FloorToInt(explosionCenter), _damage);

            foreach (var connection in _server.ClientConnections)
            {
                if (_server.TryGetPlayerData(connection, out var playerData) && playerData.IsAlive &&
                    IsExplodingPosition(connection.identity.transform.position))
                {
                    _server.Damage(_owner, connection, CalculateLinearDamage(connection.identity.transform.position));
                }
            }

            var explosives = _server.EntityContainer.Explosives;
            for (var i = 0; i < explosives.Count; i++)
            {
                if (IsExplodingPosition(explosives[i].Position))
                {
                    explosives[i].Explode();
                }
            }
        }

        private bool IsExplodingPosition(Vector3 entityPosition)
        {
            return Vector3.Distance(entityPosition, _explosionCenter) <= _radius;
        }

        private int CalculateLinearDamage(Vector3 entityPosition)
        {
            return (int) ((1 - Vector3.Distance(entityPosition, _explosionCenter) / _radius) * _damage);
        }
    }
}