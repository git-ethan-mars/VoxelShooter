using UnityEngine;

namespace Infrastructure.Services.Input
{
    public class StandaloneInputService : IInputService
    {
        public Vector2 Axis => _isEnabled
            ? new Vector2(UnityEngine.Input.GetAxisRaw("Vertical"), UnityEngine.Input.GetAxisRaw("Horizontal"))
            : Vector2.zero;

        public Vector2 MouseAxis => _isEnabled
            ? new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"))
            : Vector2.zero;

        public bool IsFirstActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(0) && _isEnabled;
        public bool IsFirstActionButtonUp() => UnityEngine.Input.GetMouseButtonUp(0) && _isEnabled;
        public bool IsFirstActionButtonHold() => UnityEngine.Input.GetMouseButton(0) && _isEnabled;
        public bool IsSecondActionButtonDown() => UnityEngine.Input.GetMouseButtonDown(1) && _isEnabled;
        public bool IsSecondActionButtonUp() => UnityEngine.Input.GetMouseButtonUp(1) && _isEnabled;
        public bool IsReloadingButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.R) && _isEnabled;
        public bool IsJumpButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Space) && _isEnabled;
        public float GetScrollSpeed() => _isEnabled ? UnityEngine.Input.GetAxis("Mouse ScrollWheel") : 0.0f;
        public bool IsScoreboardButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Tab);
        public bool IsScoreboardButtonUp() => UnityEngine.Input.GetKeyUp(KeyCode.Tab);

        public bool IsChooseClassButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.M);
        public bool IsInGameMenuButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        public bool IsLeftArrowButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) && _isEnabled;
        public bool IsRightArrowButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) && _isEnabled;
        public bool IsUpArrowButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) && _isEnabled;
        public bool IsDownArrowButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) && _isEnabled;
        public bool IsZActionButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.Z);
        public bool IsXActionButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.X);
        public bool IsCActionButtonDown() => UnityEngine.Input.GetKeyDown(KeyCode.C);

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        private bool _isEnabled;
    }
}