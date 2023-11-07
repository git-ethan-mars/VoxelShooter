using System;
using Data;
using UnityEngine;

namespace Entities
{
    public class SpawnPoint : MonoBehaviour, IPushable
    {
        public event Action<SpawnPointData, SpawnPointData> PositionUpdated;
        private SpawnPointData _data;
        private Vector3Int _size;
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);
        
        public void Construct(SpawnPointData data)
        {
            _data = data;
        }
        
        private void Awake()
        {
            var bounds = GetComponent<Collider>().bounds;
            _size = Vector3Int.RoundToInt(bounds.size);
        }
        
        public void Push()
        {
            var previousData = _data;
            transform.position += Vector3.up;
            _data = new SpawnPointData(Center);
            PositionUpdated?.Invoke(previousData, _data);
        }

        public void Fall()
        {
            transform.position += Vector3.down;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}