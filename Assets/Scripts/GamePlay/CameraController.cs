using System;
using Mirror;
using UnityEngine;

namespace GamePlay
{
    public class CameraController : NetworkBehaviour
    {
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private float sensitivityX;
        [SerializeField] private float sensitivityY;
        private Transform _cameraTransform;
        private Camera Camera { get; set; }
        private float XRotation { get; set; }
        private float YRotation { get; set; }

        public override void OnStartAuthority()
        {
            cameraObject.SetActive(true);
            Camera = cameraObject.GetComponent<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cameraTransform = Camera.transform;

        }

        private void LateUpdate()
        {
            if (!isLocalPlayer) return;
            var mouseXInput = Input.GetAxisRaw("Mouse X");
            var mouseYInput = Input.GetAxisRaw("Mouse Y");
            var mouseX = mouseXInput * Time.deltaTime * sensitivityX;
            var mouseY = mouseYInput * Time.deltaTime * sensitivityY;
            YRotation += mouseX;
            XRotation -= mouseY;
            XRotation = Math.Clamp(XRotation, -90, 90);
            _cameraTransform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
            _cameraTransform.position = transform.position; 
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        }
    }
}
