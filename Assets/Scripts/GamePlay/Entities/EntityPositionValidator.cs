using UnityEngine;
using VoxelMap;

namespace GamePlay.Entities
{
    public class EntityPositionValidator : MonoBehaviour
    {
        private MapProvider _mapProvider;

        public void Construct(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
            _mapProvider.MapUpdated += OnMapUpdate;
        }

        private void OnMapUpdate()
        {
            foreach (var pushable in Entity.GetEntitiesByType<IPushable>())
            {
                if (IsFreeSpace(pushable))
                {
                    pushable.Fall();
                }
                
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
                        if (_mapProvider.GetVoxelByGlobalPosition(pushable.Center.x + x,
                                pushable.Center.y + y, pushable.Center.z + z).IsSolid())
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void OnDestroy()
        {
            _mapProvider.MapUpdated -= OnMapUpdate;
        }
    }
}