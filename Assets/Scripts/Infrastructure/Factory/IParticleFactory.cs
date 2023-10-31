﻿using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IParticleFactory : IService
    {
        void CreateBulletImpact(Vector3 position, Quaternion rotation, Color32 blockColor);
        void CreateBlood(Vector3 position, Quaternion rotation);
        void CreateRchParticle(Vector3 position, int startSpeed, int burstCount);
        void CreateDrillParticle(Vector3 position, int startSpeed, int burstCount, Color color, Quaternion rotation);
        ParticleSystem CreateFallingMeshParticle(Transform particleContainer);
    }
}