using Steamworks;

namespace Infrastructure.PlayerDataLoader
{
	public class SteamAuthenticationService : IAuthenticationService
	{
		public string GetPlayerNickName()
		{
			return SteamFriends.GetPersonaName();
		}

		public ulong GetPlayerId()
		{
			return SteamUser.GetSteamID().m_SteamID;
		}
	}
}