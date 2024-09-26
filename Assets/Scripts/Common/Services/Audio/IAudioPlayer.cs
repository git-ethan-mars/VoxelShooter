using Common.StaticData;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Audio
{
	public interface IAudioPlayer : IService
	{
		UniTask PlayAsync(Vector3 position, AudioData audioData);
		UniTask PlayAsync(AudioData audioData, Transform parent, float spatialBlend);
	}
}