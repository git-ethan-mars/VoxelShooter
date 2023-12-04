using UnityEngine;

namespace Entities
{
    public class Tombstone : MonoBehaviour, IPushable
    {
        [SerializeField]
        private new Collider collider;

        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);

        private Vector3Int _size;

        private void Awake()
        {
            _size = Vector3Int.RoundToInt(collider.bounds.size);
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