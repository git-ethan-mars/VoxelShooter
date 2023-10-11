using System;
using Data;
using UnityEngine;

namespace Entities
{
    public class SpawnPoint : PushableObject
    {
        public event Action<SpawnPointData, SpawnPointData> PositionUpdated;
        private SpawnPointData _data;

        public void Construct(SpawnPointData data)
        {
            _data = data;
        }

        public override void Push()
        {
            var previousData = _data;
            transform.position += Vector3.up;
            _data = new SpawnPointData(Center);
            PositionUpdated?.Invoke(previousData, _data);
        }

        public override void Fall()
        {
            transform.position += Vector3.down;
        }
    }
}