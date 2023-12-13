using Newtonsoft.Json;

namespace UI.SettingsMenu
{
    public class MouseSettingsData : ISettingsData
    {
        public readonly float GeneralSensitivity;
        public readonly float AimSensitivity;

        [JsonConstructor]
        public MouseSettingsData(float generalSensitivity, float aimSensitivity)
        {
            GeneralSensitivity = generalSensitivity;
            AimSensitivity = aimSensitivity;
        }

        public MouseSettingsData()
        {
            GeneralSensitivity = 1.0f;
            AimSensitivity = 1.0f;
        }
    }
}