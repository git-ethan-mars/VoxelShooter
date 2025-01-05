using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
    public class Spectator : MonoBehaviour
    {
        private const float DistanceToPlayer = 5.0f;

        public SpectatorRotation Rotation { get; private set; }

        private IInputService _inputService;
        
        public void Construct(IInputService inputService, IStorageService storageService)
        {
            _inputService = inputService;
            Rotation = new SpectatorRotation(transform, storageService);
            MountCamera();
        }

        private void Update()
        {
            Rotation.Rotate(_inputService.MouseAxis);
        }

        private void MountCamera()
        {
            var cameraTransform = Camera.main!.transform;
            cameraTransform.SetParent(transform);
            cameraTransform.SetLocalPositionAndRotation(Vector3.back * DistanceToPlayer, Quaternion.identity);
        }
    }
}