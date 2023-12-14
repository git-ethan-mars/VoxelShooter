using System;
using System.IO;
using Newtonsoft.Json;
using UI.SettingsMenu;
using UnityEngine;

namespace Infrastructure.Services.Storage
{
    public class JsonToFileStorageService : IStorageService
    {
        public event Action<ISettingsData> DataSaved;

        public void Save<T>(string key, T data) where T : ISettingsData
        {
            var path = BuildPath(key);
            var json = JsonConvert.SerializeObject(data);
            using (var fileStream = new StreamWriter(path))
            {
                fileStream.Write(json);
            }

            DataSaved?.Invoke(data);
        }

        public T Load<T>(string key) where T : ISettingsData, new()
        {
            var path = BuildPath(key);
            if (!File.Exists(path))
            {
                var fileStream = File.Create(path);
                fileStream.Close();
            }

            using (var fileStream = new StreamReader(path))
            {
                var json = fileStream.ReadToEnd();
                var data = JsonConvert.DeserializeObject<T>(json);
                return data ?? new T();
            }
        }

        private string BuildPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key);
        }
    }
}