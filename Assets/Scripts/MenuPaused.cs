using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPaused : MonoBehaviour
{
   
    public GameObject menuPaused;
    [SerializeField] KeyCode keyMenuPaused;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject InventoryPanel;
    bool isMenuPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        menuPaused.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyMenuPaused))
        {
            isMenuPaused = !isMenuPaused;
        }

        if (isMenuPaused)
        {
            menuPaused.SetActive(true);
            crosshair.SetActive(false);
            InventoryPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }

        else
        {
            menuPaused.SetActive(false);
            crosshair.SetActive(true);
            InventoryPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
        }
    }
    public void Resume()
    {
        isMenuPaused = false;
    }

    /*public void ExitAndQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }*/
        
}
