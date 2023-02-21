using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private KeyCode keyMenuPaused;
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveAndExitButton;

        private bool IsMenuPaused { get; set; }

        private void Start()
        {
            pauseMenu.SetActive(false);
            saveAndExitButton.onClick.AddListener(ExitAndQuit);
            continueGameButton.onClick.AddListener(Resume);
        }

        public void Update()
        {
            if (!Input.GetKeyDown(keyMenuPaused)) return;
            IsMenuPaused = !IsMenuPaused;
            if (IsMenuPaused)
            {
                pauseMenu.SetActive(true);
                gameplayUI.SetActive(false);
                ChangeButtonStates(true);
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
            }
            else
            {
                Resume();
            }
        }

        private void ChangeButtonStates(bool status)
        {
            continueGameButton.gameObject.SetActive(status);
            saveAndExitButton.gameObject.SetActive(status);
            settingsButton.gameObject.SetActive(status);
        }

        private void Resume()
        {
            pauseMenu.SetActive(false);
            gameplayUI.SetActive(true);
            ChangeButtonStates(false);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            IsMenuPaused = false;
        }

        private static void ExitAndQuit()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}