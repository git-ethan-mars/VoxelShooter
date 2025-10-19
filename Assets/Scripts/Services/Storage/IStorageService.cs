using System;
namespace Services
{
	public interface IStorageService
	{
		public const string VideoSettingsKey = "video_settings";
		public const string MouseSettingsKey = "mouse_settings";
		public const string VolumeSettingsKey = "volume_settings";
		T Load<T>(string key) where T : ISettingsData, new();
		void Set<T>(T data) where T : ISettingsData;
		void Save<T>(string key) where T : ISettingsData;
		IDisposable Subscribe<T>(Action<T> callback) where T : ISettingsData;
	}
}