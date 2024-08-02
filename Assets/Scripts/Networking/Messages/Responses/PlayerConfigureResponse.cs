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
        public readonly int BlockCount;

        public PlayerConfigureResponse(float placeDistance, float speed, float jumpHeight, List<int> itemIds, int health, int blockCount)
        {
            PlaceDistance = placeDistance;
            Speed = speed;
            JumpHeight = jumpHeight;
            ItemIds = itemIds;
            Health = health;
            BlockCount = blockCount;
        }
    }
}