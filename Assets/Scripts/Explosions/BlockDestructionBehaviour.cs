using System.Collections.Generic;
using Data;
using MapLogic;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Explosions
{
    public class BlockDestructionBehaviour
    {
        private readonly BlockHealthSystem _blockHealthSystem;
        private readonly IMapProvider _mapProvider;
        private readonly MapUpdater _mapUpdater;
        private readonly IDamageBlockArea _damageArea;

        public BlockDestructionBehaviour(IServer server, IDamageBlockArea damageArea)
        {
            _blockHealthSystem = server.BlockHealthSystem;
            _mapProvider = server.MapProvider;
            _mapUpdater = server.MapUpdater;
            _damageArea = damageArea;
        }

        public void DamageBlocks(Vector3Int damageCenter, int damage)
        {
            var overlappedPositions = _damageArea.GetOverlappedBlockPositions(damageCenter);
            var changedBlocks = new List<BlockDataWithPosition>();
            for (var i = 0; i < overlappedPositions.Count; i++)
            {
                var oldBlock = _mapProvider.GetBlockByGlobalPosition(overlappedPositions[i]);
                var newBlock = _blockHealthSystem.DamageBlock(overlappedPositions[i],
                    _damageArea.CalculateBlockDamage(overlappedPositions[i], damageCenter, damage));
                if (!oldBlock.Equals(newBlock))
                {
                    changedBlocks.Add(new BlockDataWithPosition(overlappedPositions[i], newBlock));
                }
            }

            _mapUpdater.UpdateMap(changedBlocks);
        }
    }
}