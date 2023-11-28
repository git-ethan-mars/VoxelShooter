using System;
using Data;
using Infrastructure.Services.Storage;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation
    {
        private const float SensitivityMultiplier = 2.0f;

        public float Sensitivity { get; set; }

        private readonly Transform _headPivot;
        private readonly Transform _bodyOrientation;


        private float _xRotation;
        private float _yRotation;

        public PlayerRotation(Transform bodyOrientation, Transform headPivot, float sensitivity)
        {
            _bodyOrientation = bodyOrientation;
            _headPivot = headPivot;
            Sensitivity = sensitivity;
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
    }
}