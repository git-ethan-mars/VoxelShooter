using Newtonsoft.Json;

namespace UI.SettingsMenu
{
    public class VolumeSettingsData : ISettingsData
    {
        public readonly float MasterVolume;
        public readonly float MusicVolume;
        public readonly float SoundVolume;

        [JsonConstructor]
        public VolumeSettingsData(float masterVolume, float musicVolume, float soundVolume)
        {
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
        }

        public VolumeSettingsData()
        {
            MasterVolume = 0.5f;
            MusicVolume = 0.5f;
            SoundVolume = 0.5f;
        }
    }
}