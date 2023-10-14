using System;
using System.Collections;
using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IFallingMeshParticlePool : IService
    {
        ParticleSystem Get();
        IEnumerator ReleaseOnDelay(ParticleSystem particleSystem, float lifetime);
    }
}