using Mirror;
using UnityEngine;

namespace CameraLogic
{
    public class CameraMounter : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraMountPoint;
        public void Start()
        {
            if (!isLocalPlayer)
                return;
            var cameraTransform = Camera.main.gameObject.transform;
            cameraTransform.parent = cameraMountPoint.transform;
            cameraTransform.position = cameraMountPoint.transform.position;
            cameraTransform.rotation = cameraMountPoint.transform.rotation;
        }
    }
}