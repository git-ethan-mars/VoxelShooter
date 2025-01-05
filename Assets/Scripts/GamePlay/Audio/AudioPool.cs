using System;
using System.Collections.Generic;
using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Audio
{
    public class AudioPool
    {
        private const string AudioSourcePath = "Prefabs/AudioSource";
        private const string ContainerName = "AudioContainer";
        
        private readonly Stack<AudioSource> _stack;
        
        private const int PoolSize = 50;

        public AudioPool(IAssetProvider assets)
        {
            _stack = new Stack<AudioSource>(PoolSize);
            var container = new GameObject(ContainerName);
            for (var i = 0; i < PoolSize; i++)
            {
                var audioSource = assets.Instantiate(AudioSourcePath, container.transform).GetComponent<AudioSource>();
                audioSource.gameObject.SetActive(false);
                _stack.Push(audioSource);
            }
        }

        public AudioSource Get()
        {
            var audioSource = _stack.Pop();
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }

        public async UniTaskVoid ReleaseOnDelayAsync(AudioSource audioSource, TimeSpan timeSpan)
        {
            await UniTask.Delay(timeSpan);
            Release(audioSource);
        }

        private void Release(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
            audioSource.transform.SetParent(null);
            audioSource.transform.position = Vector3.zero;
            _stack.Push(audioSource);
        }
    }
}