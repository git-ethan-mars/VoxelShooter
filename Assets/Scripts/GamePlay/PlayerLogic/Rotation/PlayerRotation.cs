using System;
using Common;
using Common.Storage;
using PlayerLogic.Rotation;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation : IRotation
    {
        private const float SensitivityMultiplier = 50.0f;

        private float Sensitivity => _zoomService.IsZoomed ? _aimSensitivity : _generalSensitivity;
        
        private readonly Transform _headPivot;
        private readonly Transform _bodyOrientation;
        private readonly ZoomService _zoomService;
        
        private float _generalSensitivity;
        private float _aimSensitivity;
        private float _xRotation;
        private float _yRotation;
        private readonly IStorageService _storageService;

        public PlayerRotation(IStorageService storageService, ZoomService zoomService, Transform bodyOrientation,
            Transform headPivot)
        {
            _zoomService = zoomService;
            _storageService = storageService;
            _storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged);
            var mouseSettings = storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey);
            _generalSensitivity = mouseSettings.GeneralSensitivity;
            _aimSensitivity = mouseSettings.AimSensitivity;
            _bodyOrientation = bodyOrientation;
            _headPivot = headPivot;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Math.Clamp(_xRotation, -90, 90);
            _bodyOrientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            _headPivot.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }

        private void OnMouseSettingsChanged(MouseSettingsData mouseSettings)
        {
            _generalSensitivity = mouseSettings.GeneralSensitivity;
            _aimSensitivity = mouseSettings.AimSensitivity;
        }

        public void Dispose()
        {
            _storageService.UnSubscribe<MouseSettingsData>(OnMouseSettingsChanged);
        }
    }
}