using System;
using UnityEngine;

namespace GamePlay.Entities
{
    public abstract class LootBox : Entity, IPushable
    {
        public event Action<LootBox, Character> PickedUp;

        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);

        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);

        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);

        [SerializeField]
        private Sprite miniMapImage;

        public Sprite MiniMapImage => miniMapImage;
        
        public bool IsLanded { get; private set; }

        [SerializeField]
        private new Collider collider;

        [SerializeField]
        private GameObject parachute;

        private Vector3Int _size;

        public void Construct()
        {
            var bounds = collider.bounds;
            _size = Vector3Int.RoundToInt(bounds.size);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Chunk"))
            {
                parachute.SetActive(false);
                IsLanded = true;
            }

            if (other.gameObject.CompareTag("Player"))
            {
                var character = other.gameObject.GetComponentInParent<Character>();
                OnPickUp(character);
                PickedUp?.Invoke(this, character);
            }
        }

        public void Push()
        {
            transform.position += Vector3.up;
        }

        public void Fall()
        {
        }

        protected abstract void OnPickUp(Character receiver);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}