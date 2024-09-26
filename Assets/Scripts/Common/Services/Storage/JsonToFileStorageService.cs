using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Storage
{
    public class JsonToFileStorageService : IStorageService
    {
        public void Subscribe<T>(Action<T> callback) where T : ISettingsData
        {
            StorageHelper<T>.Subscribers.Add(callback);
        }

        public void UnSubscribe<T>(Action<T> callback) where T : ISettingsData
        {
            StorageHelper<T>.Subscribers.Remove(callback);
        }

        public void Save<T>(string key, T data) where T : ISettingsData
        {
            var path = BuildPath(key);
            var json = JsonConvert.SerializeObject(data);
            using (var fileStream = new StreamWriter(path))
            {
                fileStream.Write(json);
            }

            Notify(data);
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

        private void Notify<T>(T value) where T : ISettingsData
        {
            foreach (var callBack in StorageHelper<T>.Subscribers)
            {
                callBack.Invoke(value);
            }
        }

        private string BuildPath(string key)
        {
            return Path.Combine(Application.persistentDataPath, key);
        }

        private static class StorageHelper<T>
        {
            public static readonly HashSet<Action<T>> Subscribers = new();
        }
    }
}