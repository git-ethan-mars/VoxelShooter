using System.Collections;
using System.Collections.Generic;
using Infrastructure.Factory;
using UnityEngine;

namespace Networking.ClientServices
{
    public class AudioPool
    {
        private const string ContainerName = "AudioContainer";
        private readonly Stack<AudioSource> _stack;
        private const int PoolSize = 50;

        public AudioPool(IGameFactory gameFactory)
        {
            _stack = new Stack<AudioSource>(PoolSize);
            var container = gameFactory.CreateGameObjectContainer(ContainerName).transform;
            for (var i = 0; i < PoolSize; i++)
            {
                var audioSource = gameFactory.CreateAudioSource(container);
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

        private void Release(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
            audioSource.transform.position = Vector3.zero;
            audioSource.GetComponent<TransformFollower>().enabled = false;
            _stack.Push(audioSource);
        }
        
        public IEnumerator ReleaseOnDelay(AudioSource audioSource, float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            Release(audioSource);
        }
    }
}