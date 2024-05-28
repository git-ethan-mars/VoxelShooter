using System;

namespace Networking.Prediction
{
    public interface INetworkedClientState : IEquatable<INetworkedClientState>
    {
        /// <summary>
        /// The tick number of the last input packet processed by the server
        /// </summary>
        uint LastProcessedInputTick { get; }
    }
}