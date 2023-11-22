using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IParticleFactory : IService
    {
        void CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor);
        void CreateBlood(Vector3 position, Quaternion rotation);
        void CreateRchParticle(Vector3 position, int startSpeed, int burstCount);

        ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
        void CreateWeatherParticle(string mapName, Transform parent);
    }
}