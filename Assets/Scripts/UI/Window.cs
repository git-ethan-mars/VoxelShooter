using UnityEngine;

namespace UI
{
    public abstract class Window : MonoBehaviour
    {
        protected void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        protected void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}