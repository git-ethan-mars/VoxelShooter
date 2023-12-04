using UnityEngine;

namespace PlayerLogic.Spectator
{
    public class SpectatorRotation
    {
        private const float SensitivityMultiplier = 2.0f;
        public float Sensitivity { get; set; }

        private readonly Transform _spectator;
        private float _xRotation;
        private float _yRotation;

        public SpectatorRotation(Transform spectator, float sensitivity)
        {
            _spectator = spectator;
            Sensitivity = sensitivity;
        }

        public void Rotate(Vector2 direction)
        {
            var mouseX = direction.x * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            var mouseY = direction.y * SensitivityMultiplier * Sensitivity * Time.deltaTime;
            _yRotation += mouseX;
            _xRotation -= mouseY;
            _spectator.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        }
    }
}