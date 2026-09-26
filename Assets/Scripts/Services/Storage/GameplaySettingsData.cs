using Newtonsoft.Json;

namespace Services
{
	public class GameplaySettingsData : ISettingsData
	{
		public readonly bool AutoJump;
		public readonly bool AutoReload;

		[JsonConstructor]
		public GameplaySettingsData(bool autoJump, bool autoReload)
		{
			AutoJump = autoJump;
			AutoReload = autoReload;
		}

		public GameplaySettingsData()
		{
			AutoJump = true;
			AutoReload = false;
		}
	}
}
