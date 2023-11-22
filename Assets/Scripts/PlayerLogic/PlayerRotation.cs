using System;
using Data;
using Infrastructure.Services.Storage;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation
    {
        private const float SensitivityMultiplier = 2.0f;

        private readonly Transform _headPivot;
        private readonly Transform _bodyOrientation;
        private readonly float _sensitivity;

        private float _xRotation;
        private float _yRotation;

        public PlayerRotation(Transform bodyOrientation, Transform headPivot, float sensitivity)
        {
            _bodyOrientation = bodyOrientation;
            _headPivot = headPivot;
            _sensitivity = sensitivity;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * _sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Math.Clamp(_xRotation, -90, 90);
            _bodyOrientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            _headPivot.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
    }
}