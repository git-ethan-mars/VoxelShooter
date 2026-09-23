using Newtonsoft.Json;
namespace Services.ServerList
{
	public record Server
	{
		[JsonProperty("server_id")]
		public long ServerID { get; private set; }
		[JsonProperty("server_title")]
		public string ServerTitle { get; set; }
		[JsonProperty("map_name")]
		public string MapName { get; set; }
		[JsonProperty("connected_players")]
		public int ConnectedPlayers { get; set; }
		[JsonProperty("available_slots")]
		public int AvailableSlots { get; set; }
		[JsonProperty("steam_id_lobby")]
		public ulong SteamIDLobby { get; private set; }

		public Server(long serverID, string serverTitle, string mapName, int connectedPlayers, int availableSlots, ulong steamIDLobby)
		{
			ServerID = serverID;
			ServerTitle = serverTitle;
			MapName = mapName;
			ConnectedPlayers = connectedPlayers;
			AvailableSlots = availableSlots;
			SteamIDLobby = steamIDLobby;
		}
	}
}