using System.Collections.Generic;
using Data;
using Entities;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class EntityPositionValidator
    {
        private readonly List<PushableObject> _pushableEntities;
        private readonly IMapProvider _mapProvider;

        public EntityPositionValidator(IMapUpdater mapUpdater,
            IMapProvider mapProvider)
        {
            _pushableEntities = new List<PushableObject>();
            _mapProvider = mapProvider;
            mapUpdater.MapUpdated += OnMapUpdate;
        }

        public void AddEntity(PushableObject entity)
        {
            _pushableEntities.Add(entity);
        }


        private void OnMapUpdate(Vector3Int globalPosition, BlockData blockData)
        {
            foreach (var spawnPoint in _pushableEntities)
            {
                if (IsFreeSpace(spawnPoint))
                {
                    spawnPoint.Fall();
                }
            }

            foreach (var pushable in _pushableEntities)
            {
                while (!IsFreeSpace(pushable))
                {
                    pushable.Push();
                }
            }
        }

        private bool IsFreeSpace(PushableObject pushable)
        {
            for (var x = pushable.Min.x; x <= pushable.Max.x; x++)
            {
                for (var y = pushable.Min.y; y <= pushable.Max.y; y++)
                {
                    for (var z = pushable.Min.z; z <= pushable.Max.z; z++)
                    {
                        if (!_mapProvider.GetBlockByGlobalPosition(pushable.Center.x + x,
                                    pushable.Center.y + y, pushable.Center.z + z).Color
                                .Equals(BlockColor.empty)) return false;
                    }
                }
            }

            return true;
        }
    }
}