using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Infrastructure.Services.Storage
{
    public class JsonToFileStorageService : IStorageService
    {
        public void Save(string key, object data)
        {
            var path = BuildPath(key);
            var json = JsonConvert.SerializeObject(data);
            using (var fileStream = new StreamWriter(path))
            {
                fileStream.Write(json);
            }
        }

        public T Load<T>(string key) where T : new()
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