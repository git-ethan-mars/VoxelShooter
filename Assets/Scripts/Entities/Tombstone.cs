using UnityEngine;

namespace Entities
{
    public class Tombstone : MonoBehaviour, IPushable
    {
        private Vector3Int _size;
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);

        private void Awake()
        {
            var bounds = GetComponent<Collider>().bounds;
            _size = Vector3Int.RoundToInt(bounds.size);
        }
        
        public void Push()
        {
            transform.position += Vector3.up;
        }

        public void Fall()
        {
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}