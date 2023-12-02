using Newtonsoft.Json;

namespace Data
{
    public class MouseSettingsData
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
            GeneralSensitivity = 0.5f;
            AimSensitivity = 0.5f;
        }
    }
}