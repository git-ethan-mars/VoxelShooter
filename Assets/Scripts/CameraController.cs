using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform objectToFollow;
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    private float _xRotation;
    private float _yRotation;
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
        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Math.Clamp(_xRotation, -90, 90);
        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        transform.position = objectToFollow.position;
        objectToFollow.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}
