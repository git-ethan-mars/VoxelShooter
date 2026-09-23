using Data;
namespace Networking.Messages
{
	public struct GameSettingsResponse : IResponse
	{
		public readonly GameSettings GameSettings;

		public GameSettingsResponse(GameSettings gameSettings)
		{
			GameSettings = gameSettings;
		}
	}
}