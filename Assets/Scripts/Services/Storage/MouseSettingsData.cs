using Newtonsoft.Json;
namespace Services
{
	public class MouseSettingsData : ISettingsData
	{
		public readonly float AimSensitivity;
		public readonly int CrosshairId;
		public readonly float GeneralSensitivity;

		[JsonConstructor]
		public MouseSettingsData(float generalSensitivity, float aimSensitivity, int crosshairId)
		{
			GeneralSensitivity = generalSensitivity;
			AimSensitivity = aimSensitivity;
			CrosshairId = crosshairId;
		}

		public MouseSettingsData()
		{
			GeneralSensitivity = 1.0f;
			AimSensitivity = 1.0f;
		}
	}
}