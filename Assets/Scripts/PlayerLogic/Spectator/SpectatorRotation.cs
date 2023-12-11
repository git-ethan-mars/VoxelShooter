using Data;
using Infrastructure.Services.Storage;
using UI.SettingsMenu;
using UnityEngine;

namespace PlayerLogic.Spectator
{
    public class SpectatorRotation
    {
        private const float SensitivityMultiplier = 100.0f;

        private readonly IStorageService _storageService;
        private readonly Transform _spectator;
        private float _sensitivity;
        private float _xRotation;
        private float _yRotation;

        public SpectatorRotation(Transform spectator, IStorageService storageService)
        {
            _spectator = spectator;
            _storageService = storageService;
            _sensitivity = _storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey).GeneralSensitivity;
            _storageService.DataSaved += OnMouseSettingsChanged;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _spectator.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }

        public void OnDestroy()
        {
            _storageService.DataSaved -= OnMouseSettingsChanged;
        }

        private void OnMouseSettingsChanged(object data)
        {
            if (data is MouseSettingsData mouseSettingsData)
            {
                _sensitivity = mouseSettingsData.GeneralSensitivity;
            }
        }
    }
}