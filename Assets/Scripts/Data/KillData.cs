using Mirror;

namespace Data
{
    public class KillData
    {
        public readonly NetworkConnectionToClient Killer;
        public readonly NetworkConnectionToClient Victim;

        public KillData(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            Killer = killer;
            Victim = victim;
        }
    }
}