namespace Networking.Messages
{
	public struct MapVoteUpdateResponse : IResponse
	{
		public readonly string MapName;
		public readonly int Votes;

		public MapVoteUpdateResponse(string mapName, int votes)
		{
			MapName = mapName;
			Votes = votes;
		}
	}
}
