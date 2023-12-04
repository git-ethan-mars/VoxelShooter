using Newtonsoft.Json;
using UnityEngine;

namespace Data
{
    public class VideoSettingsData
    {
        public readonly Resolution Resolution;
        public readonly FullScreenMode ScreenMode;

        [JsonConstructor]
        public VideoSettingsData(Resolution resolution, FullScreenMode screenMode)
        {
            Resolution = resolution;
            ScreenMode = screenMode;
        }

        public VideoSettingsData()
        {
            Resolution = Screen.currentResolution;
            ScreenMode = Screen.fullScreenMode;
        }
    }
}