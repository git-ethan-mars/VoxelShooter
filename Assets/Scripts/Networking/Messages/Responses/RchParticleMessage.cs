using Mirror;
using UnityEngine;

namespace Networking.Messages.Responses
{
    public struct RchParticleResponse : NetworkMessage
    {
        public readonly Vector3 Position;
        public readonly int ParticleSpeed;
        public readonly int ParticlesCount;

        public RchParticleResponse(Vector3 position, int particleSpeed, int particlesCount)
        {
            Position = position;
            ParticleSpeed = particleSpeed;
            ParticlesCount = particlesCount;
        }
    }
}