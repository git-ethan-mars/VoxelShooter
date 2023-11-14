using Data;
using UnityEngine;

namespace Entities
{
    public class SpawnPoint : MonoBehaviour, IPushable
    {
        public SpawnPointData Data { get; private set; }
        
        private Vector3Int _size;
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);
        
        public void Construct(SpawnPointData data)
        {
            Data = data;
            _size = Vector3Int.RoundToInt(GetComponent<Collider>().bounds.size);
        }
        
        public void Push()
        {
            transform.position += Vector3.up;
            Data.position = Center;
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