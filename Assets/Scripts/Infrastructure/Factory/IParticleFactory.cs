using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IParticleFactory : IService
    {
        GameObject CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor);
        GameObject CreateBlood(Vector3 position, Quaternion rotation);
        GameObject CreateRchParticle(Vector3 position, int startSpeed, int burstCount);

        ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
        void CreateWeatherParticle(string mapName, Transform parent);
        public GameObject CreateBlockDestructionParticle(Vector3 position, Quaternion rotation, Color32 blockColor);
    }
}