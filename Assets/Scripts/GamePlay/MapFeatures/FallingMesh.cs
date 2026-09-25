using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay
{
	public class FallingMesh : MonoBehaviour
	{
		private const float BreakDelay = 0.3f;
		private const float MaxLifetime = 8.0f;
		private const int MaxParticleSystems = 60;
		private const int ParticleStartSpeed = 1;
		private const int ParticlesPerSystem = 5;

		[SerializeField] private MeshFilter meshFilter;

		private FallingMeshParticlePool _particlePool;
		private bool _isBreaking;

		public void Construct(FallingMeshParticlePool particlePool)
		{
			_particlePool = particlePool;
		}

		private void Start()
		{
			// Pieces that never hit anything (e.g. fell off the map) are removed anyway.
			Destroy(gameObject, MaxLifetime);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (_isBreaking)
			{
				return;
			}

			_isBreaking = true;
			BreakAsync(destroyCancellationToken).Forget();
		}

		private void OnDestroy()
		{
			if (meshFilter.sharedMesh != null)
			{
				Destroy(meshFilter.sharedMesh);
			}
		}

		private async UniTaskVoid BreakAsync(CancellationToken cancellationToken)
		{
			await UniTask.WaitForSeconds(BreakDelay, cancellationToken: cancellationToken);
			SpawnParticles();
			Destroy(gameObject);
		}

		// Particles come from the pool and outlive the mesh, so the piece can be destroyed right away.
		private void SpawnParticles()
		{
			Mesh mesh = meshFilter.sharedMesh;
			Vector3[] vertices = mesh.vertices;
			Color32[] colors = mesh.colors32;
			int faceCount = vertices.Length / 4;
			int faceStep = Mathf.Max(1, Mathf.CeilToInt((float)faceCount / MaxParticleSystems));

			for (int face = 0; face < faceCount; face += faceStep)
			{
				int vertex = face * 4;
				Vector3 faceCenter = (vertices[vertex] + vertices[vertex + 2]) * 0.5f;
				ParticleSystem particles = _particlePool.Get();
				ConfigureParticles(particles, transform.TransformPoint(faceCenter), colors[vertex]);
				_particlePool.ReleaseAsync(particles, particles.main.startLifetime.constant).Forget();
			}
		}

		private static void ConfigureParticles(ParticleSystem particles, Vector3 position, Color32 color)
		{
			particles.transform.position = position;
			ParticleSystem.MainModule main = particles.main;
			main.startSpeed = ParticleStartSpeed;
			// Vertex alpha stores ambient occlusion, not transparency.
			Color startColor = color;
			startColor.a = 1.0f;
			main.startColor = startColor;
			var burst = new ParticleSystem.Burst(0.0f, ParticlesPerSystem, 1, 0.05f)
			{
				probability = 1
			};

			particles.emission.SetBurst(0, burst);
			particles.Play();
		}
	}
}
