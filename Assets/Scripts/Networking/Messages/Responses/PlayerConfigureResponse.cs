using System.Collections.Generic;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct PlayerConfigureResponse : NetworkMessage
    {
        public readonly float PlaceDistance;
        public readonly float Speed;
        public readonly float JumpMultiplier;
        public readonly List<int> ItemIds;

        public PlayerConfigureResponse(float placeDistance, float speed, float jumpMultiplier, List<int> itemIds)
        {
            PlaceDistance = placeDistance;
            Speed = speed;
            JumpMultiplier = jumpMultiplier;
            ItemIds = itemIds;
        }
    }
}