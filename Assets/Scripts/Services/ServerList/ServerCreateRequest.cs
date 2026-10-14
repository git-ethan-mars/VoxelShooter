using Newtonsoft.Json;

namespace Services
{
	public record ServerCreateRequest
	{
		[JsonProperty("title")] public string Title { get; }

		[JsonProperty("map_name")] public string MapName { get; }

		[JsonProperty("occupied_slots")] public int OccupiedSlots { get; }

		[JsonProperty("available_slots")] public int AvailableSlots { get; }

		[JsonProperty("steam_id_lobby")] public ulong SteamIDLobby { get; }

		public ServerCreateRequest(string title, string mapName, int occupiedSlots, int availableSlots, ulong steamIDLobby)
		{
			Title = title;
			MapName = mapName;
			OccupiedSlots = occupiedSlots;
			AvailableSlots = availableSlots;
			SteamIDLobby = steamIDLobby;
		}
	}
}
