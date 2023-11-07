using System;
using Mirror;
using UnityEngine;

namespace Entities
{
    public class LootBox : NetworkBehaviour, IPushable
    {
        public event Action<LootBox, NetworkConnectionToClient> OnPickUp;
        private Vector3Int _size;
        public Vector3Int Center => Vector3Int.FloorToInt(transform.position);
        public Vector3Int Min => new(-_size.x / 2, -_size.y / 2, -_size.z / 2);
        public Vector3Int Max => new(_size.x / 2, _size.y / 2, _size.z / 2);

        [SerializeField]
        private GameObject parachute;

        private void OnTriggerEnter(Collider other)
        {
            if (isServer)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    var player = other.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                    OnPickUp?.Invoke(this, player);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Chunk"))
            {
                parachute.SetActive(false);
            }
        }

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