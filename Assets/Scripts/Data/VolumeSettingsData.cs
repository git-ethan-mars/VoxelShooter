using Newtonsoft.Json;

namespace Data
{
    public class VolumeSettingsData
    {
        public readonly int MasterVolume;
        public readonly int MusicVolume;
        public readonly int SoundVolume;

        [JsonConstructor]
        public VolumeSettingsData(int masterVolume, int musicVolume, int soundVolume)
        {
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SoundVolume = soundVolume;
        }

        public VolumeSettingsData()
        {
            MasterVolume = 50;
            MusicVolume = 50;
            SoundVolume = 50;
        }
    }
}