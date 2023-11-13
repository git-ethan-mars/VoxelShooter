using Mirror;
using UnityEngine;

namespace CameraLogic
{
    public class CameraMounter : NetworkBehaviour
    {
        [SerializeField] private Transform cameraMountPoint;
        public override void OnStartLocalPlayer()
        {
            var cameraTransform = Camera.main.gameObject.transform;
            cameraTransform.parent = cameraMountPoint.transform;
            cameraTransform.position = cameraMountPoint.position;
            cameraTransform.rotation = cameraMountPoint.rotation;
            Camera.main.fieldOfView = Constants.DefaultFov;
        }
    }
}