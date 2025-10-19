namespace Networking.Messages
{
	public struct MapNameResponse : IResponse
	{
		public readonly string MapName;

		public MapNameResponse(string mapName)
		{
			MapName = mapName;
		}
	}
}