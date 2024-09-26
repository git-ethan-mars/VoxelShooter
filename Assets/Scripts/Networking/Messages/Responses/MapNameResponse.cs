namespace Networking.Messages.Responses
{
	public struct MapNameResponse : IMirrorResponse
	{
		public readonly string MapName;

		public MapNameResponse(string mapName)
		{
			MapName = mapName;
		}
	}
}