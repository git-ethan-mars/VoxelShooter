namespace Networking.Messages
{
	public struct VoteRequest : IRequest
	{
		public readonly string Variant;
		
		public VoteRequest(string variant)
		{
			Variant = variant;
		}
	}
}