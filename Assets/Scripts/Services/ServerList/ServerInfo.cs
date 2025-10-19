using Newtonsoft.Json;
namespace Services
{
	public record ServerInfo
	{
		[JsonProperty("owner_id")]
		public long OwnerId { get; set; }
		[JsonProperty("server_title")]
		public string ServerTitle { get; set; }
		[JsonProperty("map_name")]
		public string MapName { get; set; }
		[JsonProperty("connected_players")]
		public byte ConnectedPlayers { get; set; }
		[JsonProperty("available_slots")]
		public byte AvailableSlots { get; set; }

		public ServerInfo(long ownerId, string serverTitle, string mapName, byte connectedPlayers, byte availableSlots)
		{
			OwnerId = ownerId;
			ServerTitle = serverTitle;
			MapName = mapName;
			ConnectedPlayers = connectedPlayers;
			AvailableSlots = availableSlots;
		}
	}
}