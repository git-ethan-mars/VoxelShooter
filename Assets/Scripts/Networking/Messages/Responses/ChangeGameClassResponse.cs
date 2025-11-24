using Data;
namespace Networking.Messages
{
	public struct ChangeGameClassResponse : IResponse
	{
		public readonly GameClass GameClass;

		public ChangeGameClassResponse(GameClass gameClass)
		{
			GameClass = gameClass;
		}
	}
}