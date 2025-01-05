using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay.MapFeatures
{
    public class FallingMesh : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float lifeTime;

        private FallingMeshParticlePool _particlePool;
        private bool _hasCollided;
        private const int DestructionTime = 4;
        private const int ParticleSystemsCountModifier = 200;
        private const int MaxVerticesForMeshCollider = 24000;

        public void Construct(FallingMeshParticlePool particlePool)
        {
            _particlePool = particlePool;
        }

        private async void OnCollisionEnter(Collision collision)
        {
            var length = meshFilter.mesh.vertexCount;
            if (length > MaxVerticesForMeshCollider)
            {
                rb.isKinematic = true;
            }

            if (_hasCollided)
            {
                return;
            }

            _hasCollided = true;
            await ProcessCollision(meshFilter.mesh, length, DestructionTime);
        }

        private async UniTask ProcessCollision(Mesh mesh, int length, float lifetime)
        {
            await UniTask.WaitForSeconds(lifetime);

            var vertices = mesh.vertices;
            var colors = mesh.colors;
            var particlesContainer = new List<ParticleSystem>();
            var blocksCount = length / 24;
            var modifier = Math.Max(Math.Round((double)blocksCount / ParticleSystemsCountModifier), 1);
            var counter = 0;
            for (var i = 0; i < length; i += 24)
            {
                if (counter % modifier == 0)
                {
                    var particles = _particlePool.Get();
                    var position = transform.localRotation * (vertices[i] + new Vector3(0.5f, -0.5f, 0.5f)) +
                                   transform.localPosition;
                    ConfigureParticles(particles, position, 1, 5, colors[i / 4]);
                    particlesContainer.Add(particles);
                }

                counter++;
            }

            meshRenderer.enabled = false;
            GetComponent<Collider>().enabled = false;
            rb.isKinematic = true;
            var tasks = particlesContainer.Select(p => _particlePool.ReleaseAsync(p, p.main.startLifetime.constant));
            await UniTask.WhenAll(tasks);
            await UniTask.WaitForSeconds(lifetime);
            Destroy(gameObject);
        }

        private void ConfigureParticles(ParticleSystem particles, Vector3 position, int startSpeed, int burstCount,
            Color meshColor)
        {
            particles.gameObject.transform.position = position;
            var main = particles.main;
            main.startSpeed = startSpeed;
            main.startColor = meshColor;
            var burst = new ParticleSystem.Burst(0f, burstCount, 1, 0.05f)
            {
                probability = 1
            };

            particles.emission.SetBurst(0, burst);
        }
    }
}