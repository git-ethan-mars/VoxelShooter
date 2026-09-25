namespace Networking.Messages
{
	public struct ChatMessageRequest : IRequest
	{
		public readonly string Text;

		public ChatMessageRequest(string text)
		{
			Text = text;
		}
	}
}
