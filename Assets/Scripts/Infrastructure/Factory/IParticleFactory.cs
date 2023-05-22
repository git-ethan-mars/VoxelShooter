using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IParticleFactory : IService
    {
        GameObject CreateBulletHole(Vector3 position, Quaternion rotation);
        GameObject CreateMuzzleFlash(Transform transform);
        GameObject CreateBlood(Vector3 position);
        void CreateRchParticle(Vector3 position);
    }
}