using Newtonsoft.Json;
using UnityEngine;

namespace UI.SettingsMenu
{
    public class VideoSettingsData : ISettingsData
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