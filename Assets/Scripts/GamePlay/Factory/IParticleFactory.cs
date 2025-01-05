using Common;
using UnityEngine;

namespace GamePlay.Factory
{
	public interface IParticleFactory : IService
	{
		ParticleSystem CreateBulletImpact(Vector3 position, Quaternion rotation, Color color);
		ParticleSystem CreateBlood(Vector3 position, Quaternion rotation);
		ParticleSystem CreateRchParticle(Vector3 position, int startSpeed, int burstCount);

		ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
		ParticleSystem CreateVoxelDestructionParticle(Vector3 position, Quaternion rotation, Color color);
	}
}