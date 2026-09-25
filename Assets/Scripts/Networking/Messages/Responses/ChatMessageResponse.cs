namespace Networking.Messages
{
	public struct ChatMessageResponse : IResponse
	{
		public readonly ChatMessageType Type;
		public readonly string Sender;
		public readonly string Text;

		public ChatMessageResponse(ChatMessageType type, string sender, string text)
		{
			Type = type;
			Sender = sender;
			Text = text;
		}
	}
}
