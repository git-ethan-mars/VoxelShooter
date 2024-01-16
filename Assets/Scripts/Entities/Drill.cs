using Data;
using Explosions;
using Infrastructure.Factory;
using Mirror;
using Networking;
using Networking.ServerServices;
using UnityEngine;

namespace Entities
{
    public class Drill : NetworkBehaviour
    {
        private ExplosionBehaviour _explosionBehaviour;
        private NetworkConnectionToClient _owner;
        private bool _isExploded;
        private AudioService _audioService;
        private AudioData _drillSound;
        private Rigidbody _rigidbody;
        private int _rotationSpeed;
        [SerializeField] public ParticleSystem particleSystem;

        public void Construct(IServer server, DrillItem drillData, IParticleFactory particleFactory, AudioService audioService)
        {
            _drillSound = drillData.impactSound;
            _audioService = audioService;
            _explosionBehaviour = new ExplosionBehaviour(server, connectionToClient, drillData.radius, drillData.damage);
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _rotationSpeed = drillData.rotationSpeed;
        }

        void FixedUpdate()
        {
            if (isServer)
            {
                _rigidbody.AddForce(Vector3.down);
                var previousZAngle = _rigidbody.rotation.eulerAngles.z;
                _rigidbody.rotation = Quaternion.LookRotation(_rigidbody.velocity)
                                      * Quaternion.Euler(new Vector3(0, -180, _rotationSpeed + previousZAngle));
            }
        }

        public void Explode()
        {
            var rocketPosition = transform.position;
            particleSystem.Play();
            _explosionBehaviour.Explode(rocketPosition);
            _audioService.SendAudio(_drillSound, rocketPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer)
            {
                return;
            }

            if (other.gameObject.GetComponentInParent<NetworkIdentity>()?.connectionToClient == connectionToClient)
            {
                return;
            }

            Explode();
        }
    }
}