using UnityEngine;

namespace Networking.Prediction.Player
{
    public readonly struct PlayerState : INetworkedClientState
    {
        public uint LastProcessedInputTick => lastProcessedInputTick;

        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
        public readonly uint lastProcessedInputTick;

        public PlayerState(Vector3 position, Vector3 velocity, uint lastProcessedInputTick)
        {
            Position = position;
            Velocity = velocity;
            this.lastProcessedInputTick = lastProcessedInputTick;
        }

        public bool Equals(INetworkedClientState other)
        {
            return other is PlayerState otherState && Equals(otherState);
        }

        private bool Equals(PlayerState other)
        {
            return Mathf.Abs(Position.sqrMagnitude - other.Position.sqrMagnitude) < Constants.Epsilon &&
                   Mathf.Abs(Velocity.sqrMagnitude - other.Velocity.sqrMagnitude) < Constants.Epsilon;
        }

        public override string ToString()
        {
            return $"Pos: {Position} | Vel: {Velocity}  | LastProcessInput: {lastProcessedInputTick}";
        }
    }
}