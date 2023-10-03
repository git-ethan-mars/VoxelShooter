using System;
using System.Collections;
using System.Collections.Generic;
using Infrastructure.Factory;
using UnityEngine;

namespace Entities
{
    public class FallingMesh : MonoBehaviour
    {
        private IFallingMeshParticlePool _particlePool;
        private bool _hasCollided;

        public void Construct(IFallingMeshParticlePool particlePool)
        {
            _particlePool = particlePool;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (_hasCollided)
                return;
            _hasCollided = true;
            StartCoroutine(ProcessCollision(2));
        }
        
        public IEnumerator ProcessCollision(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            var mesh = GetComponent<MeshFilter>().mesh;
            var particleSystems = new List<ParticleSystem>();
            var counter = 0;
            var length = mesh.vertices.Length;
            for (var i = 0; i < length; i+=24)
            {
                if (counter % (int)(length / Math.Sqrt(length)) == 0)
                {
                    particleSystems.Add(CreateFallingMeshParticle(
                        transform.localRotation * (mesh.vertices[i] + new Vector3(0.5f, -0.5f, 0.5f)) +
                        transform.localPosition,
                        1, 5, mesh.colors[i / 4]));
                }
                counter++;
            }
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            foreach (var particleSystem in particleSystems)
            {
                StartCoroutine(_particlePool.ReleaseOnDelay(particleSystem,
                    particleSystem.main.startLifetime.constant));
            }
            
            StartCoroutine(Utils.DoActionAfterDelay(7, () => Destroy(gameObject)));
        }

        private ParticleSystem CreateFallingMeshParticle(Vector3 position, int startSpeed, int burstCount, Color meshColor)
        {
            var particleSystem = _particlePool.Get();
            particleSystem.gameObject.transform.position = position;
            var main = particleSystem.main;
            main.startSpeed = startSpeed;
            main.startColor = meshColor;
            var burst = new ParticleSystem.Burst(0f, burstCount, 1, 0.05f)
            {
                probability = 1
            };
            particleSystem.emission.SetBurst(0, burst);
            return particleSystem;
        }
    }
}
