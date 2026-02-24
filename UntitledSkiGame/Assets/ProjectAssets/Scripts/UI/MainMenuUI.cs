using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    public void Play()
    {
        SceneManager.LoadScene(1); // TerrainTesting
    }

    public void Quit()
    {
        Application.Quit();
    }
}