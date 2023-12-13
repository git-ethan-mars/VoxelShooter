using System;
using System.Collections;
using System.Collections.Generic;
using Infrastructure.Factory;
using UnityEngine;

namespace Entities
{
    public class FallingMesh : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private MeshRenderer meshRenderer;

        private IFallingMeshParticlePool _particlePool;
        private bool _hasCollided;
        private const int DestructionTime = 4;
        private const int ParticleSystemsCountModifier = 200;
        private const int MaxVerticesForMeshCollider = 24000;

        public void Construct(IFallingMeshParticlePool particlePool)
        {
            _particlePool = particlePool;
        }

        public void OnCollisionEnter(Collision collision)
        {
            var length = meshFilter.mesh.vertexCount;
            if (length > MaxVerticesForMeshCollider)
                rigidbody.isKinematic = true;
            if (_hasCollided)
                return;
            _hasCollided = true;
            StartCoroutine(ProcessCollision(meshFilter.mesh, length, DestructionTime));
        }

        public IEnumerator ProcessCollision(Mesh mesh, int length, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            var vertices = mesh.vertices;
            var colors = mesh.colors;
            var particleSystems = new List<ParticleSystem>();
            var blocksCount = length / 24;
            var modifier = Math.Max(Math.Round((double) blocksCount / ParticleSystemsCountModifier), 1);
            var counter = 0;
            for (var i = 0; i < length; i += 24)
            {
                if (counter % modifier == 0)
                {
                    particleSystems.Add(CreateFallingMeshParticle(
                        transform.localRotation * (vertices[i] + new Vector3(0.5f, -0.5f, 0.5f)) +
                        transform.localPosition,
                        1, 5, colors[i / 4]));
                }

                counter++;
            }

            meshRenderer.enabled = false;
            GetComponent<Collider>().enabled = false;
            rigidbody.isKinematic = true;
            foreach (var particleSystem in particleSystems)
            {
                StartCoroutine(_particlePool.ReleaseOnDelay(particleSystem,
                    particleSystem.main.startLifetime.constant));
            }

            StartCoroutine(Utils.DoActionAfterDelay(() => Destroy(gameObject), 7));
        }

        private ParticleSystem CreateFallingMeshParticle(Vector3 position, int startSpeed, int burstCount,
            Color meshColor)
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