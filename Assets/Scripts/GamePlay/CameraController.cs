using System;
using UnityEngine;

namespace GamePlay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform objectToFollow;
        [SerializeField] private float sensitivityX;
        [SerializeField] private float sensitivityY;
        private float XRotation { get; set; }
        private float YRotation { get; set; }
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            var mouseXInput = Input.GetAxisRaw("Mouse X");
            var mouseYInput = Input.GetAxisRaw("Mouse Y");
            var mouseX = mouseXInput * Time.deltaTime * sensitivityX;
            var mouseY = mouseYInput * Time.deltaTime * sensitivityY;
            YRotation += mouseX;
            XRotation -= mouseY;
            XRotation = Math.Clamp(XRotation, -90, 90);
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
            transform.position = objectToFollow.position;
            objectToFollow.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        }
    }
}
