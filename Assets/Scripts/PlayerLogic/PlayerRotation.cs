using System;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class PlayerRotation : NetworkBehaviour
    {
        [SerializeField]
        private float sensitivityX;

        [SerializeField]
        private float sensitivityY;

        [SerializeField]
        private Transform headPivot;

        [SerializeField]
        private Transform bodyOrientation;

        private float XRotation { get; set; }
        private float YRotation { get; set; }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            var mouseXInput = Input.GetAxis("Mouse X");
            var mouseYInput = Input.GetAxis("Mouse Y");
            var mouseX = mouseXInput * sensitivityX * Time.deltaTime;
            var mouseY = mouseYInput * sensitivityY * Time.deltaTime;
            YRotation += mouseX;
            XRotation -= mouseY;
            XRotation = Math.Clamp(XRotation, -90, 90);
            bodyOrientation.rotation = Quaternion.Euler(0, YRotation, 0);
            headPivot.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        }
    }
}