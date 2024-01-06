using System.Collections.Generic;
using Entities;
using MapLogic;

namespace Networking.ServerServices
{
    public class EntityPositionValidator
    {
        private readonly List<IPushable> _pushableEntities;

        private readonly MapProvider _mapProvider;

        private readonly MapUpdater _mapUpdater;

        public EntityPositionValidator(IServer server)
        {
            _pushableEntities = new List<IPushable>();
            _mapProvider = server.MapProvider;
            _mapUpdater = server.MapUpdater;
        }

        public void Start()
        {
            _mapUpdater.MapUpdated += OnMapUpdate;
        }

        public void AddEntity(IPushable entity)
        {
            _pushableEntities.Add(entity);
        }

        public void RemoveEntity(IPushable entity)
        {
            _pushableEntities.Remove(entity);
        }

        public void Stop()
        {
            _mapUpdater.MapUpdated -= OnMapUpdate;
        }

        private void OnMapUpdate()
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

        private bool IsFreeSpace(IPushable pushable)
        {
            for (var x = pushable.Min.x; x <= pushable.Max.x; x++)
            {
                for (var y = pushable.Min.y; y <= pushable.Max.y; y++)
                {
                    for (var z = pushable.Min.z; z <= pushable.Max.z; z++)
                    {
                        if (_mapProvider.GetBlockByGlobalPosition(pushable.Center.x + x,
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