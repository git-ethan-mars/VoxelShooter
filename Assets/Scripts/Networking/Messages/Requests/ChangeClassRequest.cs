using Data;
namespace Networking.Messages
{
	public struct ChangeGameClassRequest : IRequest
	{
		public readonly GameClass GameClass;

		public ChangeGameClassRequest(GameClass gameClass)
		{
			GameClass = gameClass;
		}
	}
}