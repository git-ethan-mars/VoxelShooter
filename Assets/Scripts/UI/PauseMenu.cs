using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private KeyCode keyMenuPaused;
        [SerializeField] private GameObject saveMenu;
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveAndExitButton;
        [SerializeField] private Button confirmSaveButton;
        [SerializeField] private Button backToPauseButton;
        [SerializeField] private TMP_InputField saveName;

        private bool IsMenuPaused { get; set; }

        private void Start()
        {
            saveAndExitButton.onClick.AddListener(ExitAndQuit);
            continueGameButton.onClick.AddListener(Resume);
            confirmSaveButton.onClick.AddListener(ConfirmSave);
            backToPauseButton.onClick.AddListener(BackToPause);
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
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
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
            saveMenu.SetActive(false);
            gameplayUI.SetActive(true);
            ChangeButtonStates(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            IsMenuPaused = false;
        }

        private void ExitAndQuit()
        {
            saveMenu.SetActive(true);
            ChangeButtonStates(false);
        }

        private void ConfirmSave()
        {
            Time.timeScale = 1f;
            GlobalEvents.SendMapSave(saveName.text + ".rch");
            SceneManager.LoadScene("MainMenu");
        }

        private void BackToPause()
        {
            ChangeButtonStates(true);
            saveMenu.SetActive(false);
        }
    }
}