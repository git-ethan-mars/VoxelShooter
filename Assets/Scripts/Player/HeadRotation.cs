using System;
using Mirror;
using UnityEngine;

namespace Player
{
    public class HeadRotation : NetworkBehaviour
    {
        [SerializeField] private float sensitivityX;
        [SerializeField] private float sensitivityY;
        [SerializeField] private Transform headTransform;

        private float XRotation { get; set; }
        private float YRotation { get; set; }

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            var mouseXInput = Input.GetAxis("Mouse X");
            var mouseYInput = Input.GetAxis("Mouse Y");
            var mouseX = mouseXInput * sensitivityX * Time.deltaTime;
            var mouseY = mouseYInput * sensitivityY * Time.deltaTime;
            YRotation += mouseX;
            XRotation -= mouseY;
            XRotation = Math.Clamp(XRotation, -90, 90);
            transform.rotation = Quaternion.Euler(0, YRotation, 0);
            headTransform.rotation = Quaternion.Euler(XRotation, YRotation, 0);

        }
    }
}