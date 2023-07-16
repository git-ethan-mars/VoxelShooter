using System;
using UnityEngine;

namespace Entities
{
    public class PushableObject : MonoBehaviour
    {
        public event Action<Vector3, Vector3> PositionUpdated;
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-Size.x / 2, -Size.y / 2, -Size.z / 2);
        public Vector3Int Max => new(Size.x / 2, Size.y / 2, Size.z / 2);
        private Vector3Int Size { get; set; }

        private void Awake()
        {
            var bounds = GetComponent<Collider>().bounds;
            Size = Vector3Int.FloorToInt(bounds.size);
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            var oldPosition = transform.position;
            transform.position = newPosition;
            PositionUpdated?.Invoke(oldPosition, newPosition);
        }
    }
}