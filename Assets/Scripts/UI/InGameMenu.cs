using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameMenu : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField]
        private Button resumeButton;

        public Button ResumeButton => resumeButton;

        [SerializeField]
        private Button settingsButton;

        public Button SettingsButton => settingsButton;

        [SerializeField]
        private Button exitButton;

        public Button ExitButton => exitButton;

        public void Construct()
        {
            canvasGroup.alpha = 0.0f;
        }
    }
}