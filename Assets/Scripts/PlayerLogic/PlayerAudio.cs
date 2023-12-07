using Data;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerAudio
    {
        private const float Sound2D = 0.0f;
        private readonly AudioSource _stepAudio;

        public PlayerAudio(AudioSource stepAudio, AudioData stepAudioData)
        {
            _stepAudio = stepAudio;
            _stepAudio.clip = stepAudioData.clip;
            _stepAudio.volume = stepAudioData.volume;
            _stepAudio.minDistance = stepAudioData.minDistance;
            _stepAudio.maxDistance = stepAudioData.maxDistance;
        }

        public void EnableStepSound()
        {
            if (!_stepAudio.isPlaying)
            {
                _stepAudio.Play();
            }

            _stepAudio.loop = true;
        }

        public void DisableStepSound()
        {
            _stepAudio.loop = false;
        }

        public void DisableSpatial()
        {
            _stepAudio.spatialBlend = Sound2D;
        }
    }
}