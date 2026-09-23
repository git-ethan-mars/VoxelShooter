using System;
using System.IO;
using Newtonsoft.Json;
using R3;
using UnityEngine;
namespace Services
{
	public class JsonToFileStorageService : IStorageService
	{
		public IDisposable Subscribe<T>(Action<T> callback) where T : ISettingsData
		{
			return StorageHelper<T>.ReactiveProperty.Subscribe(callback);
		}

		public void Save<T>(string key) where T : ISettingsData
		{
			string path = BuildPath(key);
			string json = JsonConvert.SerializeObject(StorageHelper<T>.ReactiveProperty.CurrentValue);
			using (var fileStream = new StreamWriter(path))
			{
				fileStream.Write(json);
			} 
		}

		public void Set<T>(T data) where T : ISettingsData
		{
			StorageHelper<T>.ReactiveProperty.Value = data;
		}

		public T Load<T>(string key) where T : ISettingsData, new()
		{
			if (StorageHelper<T>.ReactiveProperty == null)
			{
				string path = BuildPath(key);
				if (!File.Exists(path))
				{
					FileStream fileStream = File.Create(path);
					fileStream.Close();
				}

				using (var fileStream = new StreamReader(path))
				{
					string json = fileStream.ReadToEnd();
					T data = JsonConvert.DeserializeObject<T>(json) ?? new T();
					StorageHelper<T>.ReactiveProperty = new ReactiveProperty<T>(data);
				}
			}
			
			return StorageHelper<T>.ReactiveProperty.CurrentValue;
		}

		private string BuildPath(string key)
		{
			return Path.Combine(Application.persistentDataPath, key);
		}

		private static class StorageHelper<T> where T : ISettingsData
		{
			public static ReactiveProperty<T> ReactiveProperty;
		}
	}
}