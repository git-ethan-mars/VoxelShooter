using Data;
using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IParticleFactory : IService
    {
        GameObject CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor);
        GameObject CreateMuzzleFlash(Transform transform);
        GameObject CreateBlood(Vector3 position, Quaternion rotation);
        void CreateRchParticle(Vector3 position, int startSpeed, int burstCount);

        ParticleSystem CreateFallingMeshParticle();
    }
}