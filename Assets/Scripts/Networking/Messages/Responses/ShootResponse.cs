namespace Networking.Messages.Responses
{
	public struct ShootResultResponse : IMirrorResponse
	{
		public readonly int SlotIndex;
		public readonly int BulletsInMagazine;

		public ShootResultResponse(int slotIndex, int bulletsInMagazine)
		{
			SlotIndex = slotIndex;
			BulletsInMagazine = bulletsInMagazine;
		}
	}
}