using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class StandaloneInputService : IInputService
    {
        public Vector2 Axis =>
            new(UnityEngine.Input.GetAxisRaw("Vertical"), UnityEngine.Input.GetAxisRaw("Horizontal"));
        public Vector2 MouseAxis => new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
        public bool IsFirstActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(0);
        public bool IsFirstActionButtonUp() => UnityEngine.Input.GetMouseButtonUp(0);
        public bool IsFirstActionButtonHold() => UnityEngine.Input.GetMouseButton(0);
        public bool IsSecondActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(1);
        public bool IsSecondActionButtonUp() => UnityEngine.Input.GetMouseButtonUp(1);
        public bool IsReloadingButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.R);
        public bool IsJumpButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Space);
        public float GetScrollSpeed() => UnityEngine.Input.GetAxis("Mouse ScrollWheel");
        public bool IsScoreboardButtonHold() => UnityEngine.Input.GetKey(KeyCode.Tab);
        public bool IsChooseClassButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.M);
    }
}