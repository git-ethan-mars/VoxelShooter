using GamePlay.Data;

namespace Networking.Messages.Requests
{
	public struct ChangeClassRequest : IMirrorRequest
	{
		public readonly GameClass GameClass;

		public ChangeClassRequest(GameClass gameClass)
		{
			GameClass = gameClass;
		}
	}
}