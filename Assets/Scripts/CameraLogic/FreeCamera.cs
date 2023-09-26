using Infrastructure.Services.Input;
using UnityEngine;

namespace CameraLogic
{
    public class FreeCamera : MonoBehaviour
    {
        [SerializeField]
        private float multiplier;

        [SerializeField]
        private float sensitivityX;

        [SerializeField]
        private float sensitivityY;

        private float YRotation;
        private float XRotation;

        private IInputService _inputService;

        private void Awake()
        {
            _inputService = new StandaloneInputService();
        }

        private void Update()
        {
            var movementDirection = (_inputService.Axis.x * transform.forward + _inputService.Axis.y * transform.right)
                .normalized;
            transform.Translate(movementDirection * (multiplier * Time.deltaTime), Space.World);
            var mouseXInput = _inputService.GetMouseHorizontalAxis();
            var mouseYInput = _inputService.GetMouseVerticalAxis();
            var mouseX = mouseXInput * sensitivityX * Time.deltaTime;
            var mouseY = mouseYInput * sensitivityY * Time.deltaTime;
            YRotation += mouseX;
            XRotation -= mouseY;
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
        }
    }
}