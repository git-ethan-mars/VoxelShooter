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
        private float XRotation { get; set; }
        private float YRotation { get; set; }

        public override void OnStartAuthority()
        {
            cameraObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

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
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        }
    }
}
