using Newtonsoft.Json;

namespace Services
{
	public record Server
	{
		[JsonProperty("id")] public long ID { get; private set; }

		[JsonProperty("title")] public string Title { get; private set; }

		[JsonProperty("map_name")] public string MapName { get; private set; }

		[JsonProperty("occupied_slots")] public int OccupiedSlots { get; private set; }

		[JsonProperty("available_slots")] public int AvailableSlots { get; private set; }

		[JsonProperty("steam_id_lobby")] public ulong? SteamIDLobby { get; private set; }
	}
}
