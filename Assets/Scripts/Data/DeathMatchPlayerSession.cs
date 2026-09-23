using System;
using Mirror;

namespace Data
{
    public class DeathMatchPlayerSession
    {
        public NetworkConnectionToClient Connection { get; }
        public DeathMatchPlayerData Data { get; set; }
        public bool IsAlive { get; set; }
        public TimeSpan RespawnTime { get; set; }

        public DeathMatchPlayerSession(NetworkConnectionToClient connection, DeathMatchPlayerData data)
        {
            Connection = connection;
            Data = data;
        }
    }
}