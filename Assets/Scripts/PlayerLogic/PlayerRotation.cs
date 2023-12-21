using System;
using CameraLogic;
using Infrastructure.Services.Storage;
using UI.SettingsMenu;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation
    {
        private const float SensitivityMultiplier = 50.0f;
        private const float HeadRotationLimit = 88.5f;

        private float Sensitivity => _zoomService.IsZoomed ? _aimSensitivity : _generalSensitivity;

        private readonly ZoomService _zoomService;
        private readonly Transform _trackObjectPivot;

        private float _generalSensitivity;
        private float _aimSensitivity;
        private float _xRotation;
        private float _yRotation;

        public PlayerRotation(IStorageService storageService, ZoomService zoomService, Transform cameraMountPoint)
        {
            _zoomService = zoomService;
            _trackObjectPivot = cameraMountPoint;
            var mouseSettings = storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey);
            _generalSensitivity = mouseSettings.GeneralSensitivity;
            _aimSensitivity = mouseSettings.AimSensitivity;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Math.Clamp(_xRotation, -HeadRotationLimit, HeadRotationLimit);
            _trackObjectPivot.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }

        public void ChangeMouseSettings(MouseSettingsData mouseSettings)
        {
            _generalSensitivity = mouseSettings.GeneralSensitivity;
            _aimSensitivity = mouseSettings.AimSensitivity;
        }
    }
}