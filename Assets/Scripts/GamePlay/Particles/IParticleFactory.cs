using UnityEngine;
namespace GamePlay
{
	public interface IParticleFactory
	{
		ParticleSystem CreateBulletImpact(Vector3 position, Quaternion rotation, Color color);
		ParticleSystem CreateBlood(Vector3 position, Quaternion rotation);
		ParticleSystem CreateRchParticle(Vector3 position, int startSpeed, int burstCount, float radius);

		ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
		ParticleSystem CreateVoxelDestructionParticle(Vector3 position, Quaternion rotation, Color color);
	}
}