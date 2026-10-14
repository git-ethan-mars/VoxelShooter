using Newtonsoft.Json;

namespace Services
{
	// PATCH semantics: null fields are not sent and stay unchanged on the backend.
	public record ServerUpdateRequest
	{
		[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
		public string Title { get; }

		[JsonProperty("map_name", NullValueHandling = NullValueHandling.Ignore)]
		public string MapName { get; }

		[JsonProperty("occupied_slots", NullValueHandling = NullValueHandling.Ignore)]
		public int? OccupiedSlots { get; }

		public ServerUpdateRequest(string title = null, string mapName = null, int? occupiedSlots = null)
		{
			Title = title;
			MapName = mapName;
			OccupiedSlots = occupiedSlots;
		}
	}
}
