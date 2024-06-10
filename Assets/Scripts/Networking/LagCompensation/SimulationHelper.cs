using System.Collections.Generic;

namespace Networking.LagCompensation
{
    public static class SimulationHelper
    {
        public const int BufferSize = 30;
        public static readonly List<LagCompensated> simulationObjects = new();
    }
}