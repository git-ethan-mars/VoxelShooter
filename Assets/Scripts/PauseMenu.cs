using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
   
    [SerializeField] private KeyCode keyMenuPaused;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button continueGameButton;
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
            crosshair.SetActive(false);
            inventoryPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }

        else
        {
            pauseMenu.SetActive(false);
            crosshair.SetActive(true);
            inventoryPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }
    private void Resume()
    {
        IsMenuPaused = false;
    }

    private void ExitAndQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
        
}
