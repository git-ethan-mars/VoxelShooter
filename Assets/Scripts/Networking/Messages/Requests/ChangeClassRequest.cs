using Data;
using Mirror;
using Steamworks;

namespace Networking.Messages.Requests
{
    public struct ChangeClassRequest : NetworkMessage
    {
        public readonly GameClass GameClass;
        public readonly CSteamID SteamID;
        public readonly string Nickname;

        public ChangeClassRequest(CSteamID steamID, GameClass gameClass, string nickname)
        {
            SteamID = steamID;
            GameClass = gameClass;
            Nickname = nickname;
        }
    }
}