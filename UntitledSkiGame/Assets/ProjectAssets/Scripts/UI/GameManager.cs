using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    //singleton information//
    public static GameManager instance;

    private void Awake()
    {
        if(instance)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    [Header("UI")]
    [SerializeField] public GameObject pauseMenuUI;
    [SerializeField] public GameObject debugHUD;     
    [SerializeField] public GameObject scoreCanvas;
    [SerializeField] public GameObject NPC_Dialogue;

    private bool isPaused;
    private void Start()
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


    //FUNCTIONS FOR NPC DIALOGUE//

    public GameObject EnableNPCDialogue()
    {
        //disable the camera movement
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 0;

        //enable dialogue, disable player
        NPC_Dialogue.SetActive(true);
        SkiMovement.Instance.gameObject.SetActive(false);

        //returns the canvas to be used by the script
        return NPC_Dialogue;
    }

    public void DisableNPCDialogue()
    {
        NPC_Dialogue.SetActive(false);
        SkiMovement.Instance.gameObject.SetActive(true);

        //make it so camera blends again
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 2f;
    }
}