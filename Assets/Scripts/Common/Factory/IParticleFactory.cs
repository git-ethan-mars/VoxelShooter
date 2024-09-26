using UnityEngine;

namespace Common.Factory
{
    public interface IParticleFactory : IService
    {
        ParticleSystem CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor);
        ParticleSystem CreateBlood(Vector3 position, Quaternion rotation);
        ParticleSystem CreateRchParticle(Vector3 position, int startSpeed, int burstCount);

        ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
        ParticleSystem CreateWeatherParticle(string mapName, Transform parent);
        ParticleSystem CreateBlockDestructionParticle(Vector3 position, Quaternion rotation, Color32 blockColor);
    }
}