using UnityEngine;

namespace Entities
{
    public abstract class PushableObject : MonoBehaviour
    {
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);
        private Vector3Int _size;

        private void Awake()
        {
            var bounds = GetComponent<Collider>().bounds;
            _size = Vector3Int.RoundToInt(bounds.size);
        }

        public abstract void Push();
        public abstract void Fall();
        
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}