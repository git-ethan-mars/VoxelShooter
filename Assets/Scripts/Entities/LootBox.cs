using System;
using Mirror;
using UnityEngine;

namespace Entities
{
    public class LootBox : NetworkBehaviour
    {
        public event Action<LootBox, NetworkConnectionToClient> OnPickUp;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                OnPickUp?.Invoke(this, player);
            }
        }
    }
}