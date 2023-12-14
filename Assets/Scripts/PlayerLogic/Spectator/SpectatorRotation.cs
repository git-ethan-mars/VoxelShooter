using Infrastructure.Services.Storage;
using UI.SettingsMenu;
using UnityEngine;

namespace PlayerLogic.Spectator
{
    public class SpectatorRotation
    {
        private const float SensitivityMultiplier = 50.0f;

        private readonly Transform _spectator;
        private float _sensitivity;
        private float _xRotation;
        private float _yRotation;

        public SpectatorRotation(Transform spectator, IStorageService storageService)
        {
            _spectator = spectator;
            _sensitivity = storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey).GeneralSensitivity;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _spectator.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }

        public void ChangeMouseSettings(MouseSettingsData mouseSettings)
        {
            _sensitivity = mouseSettings.GeneralSensitivity;
        }
    }
}