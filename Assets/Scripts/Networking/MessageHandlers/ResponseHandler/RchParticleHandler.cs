using Entities;
using Infrastructure.Factory;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class RchParticleHandler : ResponseHandler<RchParticleResponse>
    {
        private readonly IParticleFactory _particleFactory;

        public RchParticleHandler(IParticleFactory particleFactory)
        {
            _particleFactory = particleFactory;
        }
        protected override void OnResponseReceived(RchParticleResponse response)
        {
            _particleFactory.CreateRchParticle(response.Position, response.ParticleSpeed, response.ParticlesCount);
        }
    }
}