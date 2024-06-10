using Data;
using Explosions;
using MapLogic;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Tests.EditMode
{
    public class MockShootValidator : IRangeWeaponValidator
    {
        private readonly IServer _server;
        private readonly BlockDestructionBehaviour _blockDestructionBehaviour;

        public MockShootValidator(IServer server, IMapProvider mapProvider)
        {
            _server = server;
            var blockArea = new SingleBlockArea(mapProvider);
            _blockDestructionBehaviour =
                new BlockDestructionBehaviour(server, blockArea);
        }
        
        public void Shoot(NetworkConnectionToClient connection, Ray ray, bool requestIsButtonHolding, int tick)
        {
            var player = _server.GetPlayerData(connection);
            var weapon = player.SelectedItem as RangeWeaponItem;
            var result = Physics.Raycast(ray, out var hitInfo, weapon!.range, Constants.attackMask);
            if (!result || !hitInfo.collider.CompareTag("Chunk"))
            {
                return;
            }
            
            var blockPosition = Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal / 2);
            Debug.Log(blockPosition);
            _blockDestructionBehaviour.DamageBlocks(blockPosition, weapon.damage);
        }

        public void CancelShoot(NetworkConnectionToClient connection)
        {
        }

        public void Reload(NetworkConnectionToClient connection)
        {
        }
    }
}