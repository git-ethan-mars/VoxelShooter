namespace Networking.Messages
{
	public struct VoteResponse : IResponse
	{
		public readonly string Title;
		public readonly string[] Variants;

		public VoteResponse(string title, string[] variants)
		{
			Title = title;
			Variants = variants;
		}
	}
}
