using UnityEngine;

namespace Networking.Prediction.Player
{
    public readonly struct PlayerState : INetworkedClientState
    {
        public uint LastProcessedInputTick => lastProcessedInputTick;

        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Velocity;
        public readonly uint lastProcessedInputTick;

        public PlayerState(Vector3 position, Quaternion rotation, Vector3 velocity, uint lastProcessedInputTick)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            this.lastProcessedInputTick = lastProcessedInputTick;
        }

        public bool Equals(INetworkedClientState other)
        {
            return other is PlayerState otherState && Equals(otherState);
        }

        private bool Equals(PlayerState other)
        {
            return Position.Equals(other.Position) && Velocity.Equals(other.Velocity) &&
                   Rotation.Equals(other.Rotation);
        }

        public override string ToString()
        {
            return $"Pos: {Position} | Vel: {Velocity} | Rot: {Rotation} | LastProcessInput: {lastProcessedInputTick}";
        }
    }
}