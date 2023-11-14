using System.Collections.Generic;
using Mirror;

namespace Networking.Messages.Responses
{
    public struct PlayerConfigureResponse : NetworkMessage
    {
        public readonly float PlaceDistance;
        public readonly float Speed;
        public readonly float JumpHeight;
        public readonly List<int> ItemIds;
        public readonly int Health;

        public PlayerConfigureResponse(float placeDistance, float speed, float jumpHeight, List<int> itemIds, int health)
        {
            PlaceDistance = placeDistance;
            Speed = speed;
            JumpHeight = jumpHeight;
            ItemIds = itemIds;
            Health = health;
        }
    }
}