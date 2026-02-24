using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject debugHUD;     
    [SerializeField] private GameObject scoreCanvas;  

    private bool isPaused;

    private void Awake()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);

        if (debugHUD != null) debugHUD.SetActive(false);
        if (scoreCanvas != null) scoreCanvas.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        if (debugHUD != null) debugHUD.SetActive(true);
        if (scoreCanvas != null) scoreCanvas.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}