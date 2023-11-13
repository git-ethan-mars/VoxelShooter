using System;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation
    {
        private const float SensitivityX = 200;
        private const float SensitivityY = 200;

        private readonly Transform _headPivot;
        private readonly Transform _bodyOrientation;

        private float _xRotation;
        private float _yRotation;

        public PlayerRotation(Transform bodyOrientation, Transform headPivot)
        {
            _bodyOrientation = bodyOrientation;
            _headPivot = headPivot;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityX * Time.deltaTime;
            var mouseY = direction.y * SensitivityY * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Math.Clamp(_xRotation, -90, 90);
            _bodyOrientation.rotation = Quaternion.Euler(0, _yRotation, 0);
            _headPivot.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
    }
}