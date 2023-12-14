using Mirror;

namespace Networking.Messages.Responses
{
    public struct RocketSpawnResponse : NetworkMessage
    {
        public readonly int RocketsInSlotsCount;
        public RocketSpawnResponse(int rocketsInSlotsCount)
        {
            RocketsInSlotsCount = rocketsInSlotsCount;
        }
    }
}