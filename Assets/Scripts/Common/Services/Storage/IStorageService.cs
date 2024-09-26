using System;

namespace Common.Storage
{
    public interface IStorageService : IService
    {
        void Save<T>(string key, T data) where T : ISettingsData;
        T Load<T>(string key) where T : ISettingsData, new();
        void Subscribe<T>(Action<T> callback) where T : ISettingsData;
        void UnSubscribe<T>(Action<T> callback) where T : ISettingsData;
    }
}