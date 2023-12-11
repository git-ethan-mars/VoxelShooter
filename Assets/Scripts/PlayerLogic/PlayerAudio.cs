using Data;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerAudio
    {
        private readonly AudioSource _stepAudio;
        private readonly float _initialStepVolume;
        private readonly AudioSource _continuousAudio;
        private float _initialContinuousVolume;
        private float _soundMultiplier;

        public PlayerAudio(AudioSource stepAudio, AudioData stepAudioData, AudioSource continuousAudio)
        {
            _stepAudio = stepAudio;
            _continuousAudio = continuousAudio;
            _stepAudio.clip = stepAudioData.clip;
            _stepAudio.volume = stepAudioData.volume;
            _stepAudio.minDistance = stepAudioData.minDistance;
            _stepAudio.maxDistance = stepAudioData.maxDistance;
            _initialStepVolume = stepAudioData.volume;
        }

        public void ChangeSoundMultiplier(float soundSetting)
        {
            _soundMultiplier = soundSetting;
            _stepAudio.volume = _initialStepVolume * _soundMultiplier;
            _continuousAudio.volume = _initialContinuousVolume * _soundMultiplier;
        }

        public void ChangeContinuousAudio(AudioData audio, float spatialBlend)
        {
            _continuousAudio.clip = audio.clip;
            _continuousAudio.minDistance = audio.minDistance;
            _continuousAudio.maxDistance = audio.maxDistance;
            _continuousAudio.spatialBlend = spatialBlend;
            _initialContinuousVolume = audio.volume;
            _continuousAudio.volume = _initialContinuousVolume * _soundMultiplier;
            _continuousAudio.loop = true;
            _continuousAudio.Play();
        }

        public void StopContinuousSound()
        {
            _continuousAudio.loop = false;
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
    }
}