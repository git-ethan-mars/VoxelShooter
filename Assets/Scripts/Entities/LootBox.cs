using System;
using Mirror;
using Networking.Synchronization;
using UnityEngine;

namespace Entities
{
    public class LootBox : NetworkBehaviour
    {
        private Rigidbody rbGO;
        
        public void Construct()
        {
            rbGO = gameObject.GetComponent<Rigidbody>();
        }

        public void Update()
        {
            rbGO.WakeUp();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.tag);
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log(2);
                var receiver = other.gameObject.GetComponentInParent<NetworkIdentity>().connectionToClient;
                if (receiver.identity.TryGetComponent<HealthSynchronization>(out var health))
                    health.Damage(receiver, receiver, -100);
            }
        }
    }
}