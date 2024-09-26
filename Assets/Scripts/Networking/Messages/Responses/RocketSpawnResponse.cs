namespace Networking.Messages.Responses
{
	public struct RocketSpawnResponse : IMirrorResponse
	{
		public readonly int SlotIndex;
		public readonly int ChargedRockets;

		public RocketSpawnResponse(int slotIndex, int chargedRockets)
		{
			SlotIndex = slotIndex;
			ChargedRockets = chargedRockets;
		}
	}
}