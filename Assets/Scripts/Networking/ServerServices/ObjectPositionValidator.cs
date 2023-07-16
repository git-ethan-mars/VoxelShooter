using System.Collections.Generic;
using Data;
using Entities;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class ObjectPositionValidator
    {
        private readonly List<PushableObject> _pushableObjects;
        private readonly IMapProvider _mapProvider;

        public ObjectPositionValidator(IMapUpdater mapUpdater,
            IMapProvider mapProvider)
        {
            _pushableObjects = new List<PushableObject>();
            _mapProvider = mapProvider;
            mapUpdater.MapUpdated += OnMapUpdate;
        }

        public void AddPushable(PushableObject pushableObject)
        {
            _pushableObjects.Add(pushableObject);
        }

        private void OnMapUpdate(Vector3Int globalPosition, BlockData blockData)
        {
            foreach (var pushable in _pushableObjects)
            {
                while (!IsFreeSpace(pushable))
                {
                    pushable.transform.position += Vector3.up;
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