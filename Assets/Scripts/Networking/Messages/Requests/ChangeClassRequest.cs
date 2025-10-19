using Data;
namespace Networking.Messages
{
	public struct ChangeClassRequest : IRequest
	{
		public readonly GameClass GameClass;

		public ChangeClassRequest(GameClass gameClass)
		{
			GameClass = gameClass;
		}
	}
}