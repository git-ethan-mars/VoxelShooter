using System;
using UI.SettingsMenu;

namespace Infrastructure.Services.Storage
{
    public interface IStorageService : IService
    {
        void Save<T>(string key, T data) where T : ISettingsData;
        T Load<T>(string key) where T : ISettingsData, new();
        event Action<object> DataSaved;
    }
}