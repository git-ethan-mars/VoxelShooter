using Newtonsoft.Json;

namespace Data
{
    public class MouseSettingsData
    {
        public readonly int GeneralSensitivity;
        public readonly int AimSensitivity;

        [JsonConstructor]
        public MouseSettingsData(int generalSensitivity, int aimSensitivity)
        {
            GeneralSensitivity = generalSensitivity;
            AimSensitivity = aimSensitivity;
        }

        public MouseSettingsData()
        {
            GeneralSensitivity = 50;
            AimSensitivity = 50;
        }
    }
}