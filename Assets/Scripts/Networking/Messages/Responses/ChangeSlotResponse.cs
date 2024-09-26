namespace Networking.Messages.Responses
{
	public struct ChangeSlotResponse : IMirrorResponse
	{
		public readonly int Index;

		public ChangeSlotResponse(int index)
		{
			Index = index;
		}
	}
}