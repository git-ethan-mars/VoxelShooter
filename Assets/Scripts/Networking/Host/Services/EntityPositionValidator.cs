using System.Collections.Generic;
using Entities;

namespace Networking.Host.Services
{
    public class EntityPositionValidator
    {
        private readonly MapUpdater _mapUpdater;
        private readonly IEnumerable<IPushable> _pushableEntities;

        public EntityPositionValidator(MapUpdater mapUpdater)
        {
            _mapUpdater = mapUpdater;
        }

        public void Start()
        {
            _mapUpdater.MapUpdated += OnMapUpdate;
        }

        public void Stop()
        {
            _mapUpdater.MapUpdated -= OnMapUpdate;
        }

        private void OnMapUpdate()
        {
            foreach (var pushable in Entity.GetEntitiesByType<IPushable>())
            {
                if (IsFreeSpace(pushable))
                {
                    pushable.Fall();
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

        private bool IsFreeSpace(IPushable pushable)
        {
            for (var x = pushable.Min.x; x <= pushable.Max.x; x++)
            {
                for (var y = pushable.Min.y; y <= pushable.Max.y; y++)
                {
                    for (var z = pushable.Min.z; z <= pushable.Max.z; z++)
                    {
                        if (_mapUpdater.GetBlockByGlobalPosition(pushable.Center.x + x,
                                pushable.Center.y + y, pushable.Center.z + z).IsSolid())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}