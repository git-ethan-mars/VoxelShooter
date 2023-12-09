using Data;
using Infrastructure.Services.Input;
using Infrastructure.Services.Storage;
using Mirror;
using Networking;
using UnityEngine;

namespace PlayerLogic.Spectator
{
    public class SpectatorPlayer : NetworkBehaviour
    {
        private const float DistanceToPlayer = 5.0f;
        private IInputService _inputService;
        private NetworkIdentity _target;
        private Vector3 _staticMapPosition;
        private bool _messageSent;
        private IServer _server;
        private SpectatorRotation _rotation;
        private Camera _camera;
        private bool _isInitialized;

        public void Construct(IInputService inputService, IStorageService storageService)
        {
            _inputService = inputService;
            _camera = Camera.main;
            MountCamera();
            var sensitivity = storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey).GeneralSensitivity;
            _rotation = new SpectatorRotation(transform, sensitivity);
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            _rotation.Rotate(_inputService.MouseAxis);
        }

        public override void OnStopLocalPlayer()
        {
            _camera.transform.SetParent(null);
        }

        private void MountCamera()
        {
            var cameraTransform = _camera.transform;
            cameraTransform.SetParent(transform);
            cameraTransform.SetLocalPositionAndRotation(Vector3.back * DistanceToPlayer, Quaternion.identity);
        }
    }
}