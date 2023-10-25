using Mirror;
using Steamworks;

namespace Networking
{
    public struct AuthenticationRequest : NetworkMessage
    {
        public readonly CSteamID SteamID;
        public readonly string NickName;

        public AuthenticationRequest(CSteamID steamID, string nickName)
        {
            SteamID = steamID;
            NickName = nickName;
        }
    }
}