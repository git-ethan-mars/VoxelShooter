using UnityEngine;

namespace Networking.Prediction.Player
{
    public readonly struct PlayerInput : INetworkedClientInput
    {
        public float DeltaTime => deltaTime;

        public uint Tick => tick;

        public readonly Vector2 Direction;
        public readonly Vector3 Forward;
        public readonly Vector3 Right;
        public readonly bool JumpButtonPressed;
        public readonly float deltaTime;
        public readonly uint tick;
        
        public PlayerInput(Vector2 direction, Vector3 forward, Vector3 right, bool jumpButtonPressed, float deltaTime, uint tick)
        {
            Direction = direction;
            Forward = forward;
            Right = right;
            JumpButtonPressed = jumpButtonPressed;
            this.deltaTime = deltaTime;
            this.tick = tick;
        }
    }
}