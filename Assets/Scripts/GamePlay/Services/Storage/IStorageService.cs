using System;
using Common;

namespace GamePlay.Services
{
    public interface IStorageService : IService
    {
        public const string VideoSettingsKey = "video_settings";
        public const string MouseSettingsKey = "mouse_settings";
        public const string VolumeSettingsKey = "volume_settings";
        void Save<T>(string key, T data) where T : ISettingsData;
        T Load<T>(string key) where T : ISettingsData, new();
        void Subscribe<T>(Action<T> callback) where T : ISettingsData;
        void UnSubscribe<T>(Action<T> callback) where T : ISettingsData;
    }
}