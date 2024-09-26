using Common.Input;
using Common.Storage;
using UnityEngine;

namespace PlayerLogic.Spectator
{
    public class SpectatorPlayer : MonoBehaviour
    {
        private const float DistanceToPlayer = 5.0f;

        public SpectatorRotation Rotation { get; private set; }

        private IInputService _inputService;
        private Camera _camera;

        public void Construct(IInputService inputService, IStorageService storageService)
        {
            _inputService = inputService;
            Rotation = new SpectatorRotation(transform, storageService);
        }

        private void Start()
        {
            MountCamera();
        }

        private void Update()
        {
            Rotation.Rotate(_inputService.MouseAxis);
        }

        private void OnDestroy()
        {
            _camera.transform.SetParent(null);
        }

        private void MountCamera()
        {
            _camera = Camera.main;
            var cameraTransform = _camera!.transform;
            cameraTransform.SetParent(transform);
            cameraTransform.SetLocalPositionAndRotation(Vector3.back * DistanceToPlayer, Quaternion.identity);
        }
    }
}