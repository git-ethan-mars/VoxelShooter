namespace Networking.Messages.Requests
{
	public struct ChangeSlotRequest : IMirrorRequest
	{
		public readonly int Index;

		public ChangeSlotRequest(int index)
		{
			Index = index;
		}
	}
}