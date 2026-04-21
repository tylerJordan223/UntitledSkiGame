using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Cinemachine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

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

    [Header("Dialogue")]
    [SerializeField] public GameObject NPC_Dialogue;
    [SerializeField] public TextMeshProUGUI NPC_Dialogue_text;

    [Header("Choice")]
    [SerializeField] public GameObject choiceCanvas;
    [SerializeField] public TextMeshProUGUI QuestTitle;
    [SerializeField] public TextMeshProUGUI QuestDesc;
    [SerializeField] public TextMeshProUGUI QuestTime;
    [SerializeField] public Button AcceptButton;
    [SerializeField] public Button DeclineButton;

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
    public TextMeshProUGUI EnableNPCDialogue()
    {
        //disable the camera movement
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 0;

        //enable dialogue, disable player
        NPC_Dialogue.SetActive(true);
        SkiMovement.Instance.gameObject.SetActive(false);

        //returns the canvas to be used by the script
        return NPC_Dialogue_text;
    }

    public void DisableNPCDialogue()
    {
        NPC_Dialogue.SetActive(false);
        SkiMovement.Instance.gameObject.SetActive(true);

        //make it so camera blends again
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 2f;
    }

    //FUNCTIONS FOR CHOICE SYSTEM//

    public void OnEnableChoiceMenu(string title, string description, float time, UnityAction acceptFunction, UnityAction declineFunction)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //assign all the values
        QuestTitle.text = title;
        QuestDesc.text = description;
        QuestTime.text = $"Time: {time}";

        //set the yes button
        AcceptButton.onClick.AddListener(acceptFunction);
        AcceptButton.onClick.AddListener(DisableChoiceMenu);

        //always close dialogue with no button
        DeclineButton.onClick.AddListener(declineFunction);
        DeclineButton.onClick.AddListener(DisableChoiceMenu);

        //enable the menu
        choiceCanvas.SetActive(true);
    }

    public void DisableChoiceMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        choiceCanvas.SetActive(false);
    }
}