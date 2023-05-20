using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class StandaloneInputService : IInputService
    {
        public Vector2 Axis =>
            new Vector2(UnityEngine.Input.GetAxisRaw("Vertical"), UnityEngine.Input.GetAxisRaw("Horizontal"));

        public bool IsFirstActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(0);
        public bool IsSecondActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(1);
        public bool IsReloadingButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.R);
        public bool IsJumpButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Space);
        public bool IsFirstActionButtonHold() => UnityEngine.Input.GetMouseButton(0);
        public float GetScrollSpeed() => UnityEngine.Input.GetAxis("Mouse ScrollWheel");
        public float GetMouseHorizontalAxis() => UnityEngine.Input.GetAxis("Mouse X");
        public float GetMouseVerticalAxis() => UnityEngine.Input.GetAxis("Mouse Y");
        public bool IsScoreboardButtonHold() => UnityEngine.Input.GetKey(KeyCode.Tab);
        public bool IsChooseClassButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.M);
    }
}