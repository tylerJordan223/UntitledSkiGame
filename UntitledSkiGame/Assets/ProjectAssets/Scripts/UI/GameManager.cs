using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance)
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

    [Header("SCORE")]
    public float current_score;

    private bool isPaused;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
                AudioManager.instance.UnPauseSFX();
            }
            else 
            {
                Pause();
                AudioManager.instance.PauseSFX();
            }
        }
    }

    private void Pause()
    {
        PauseMenuUI.Instance.Show();
        if (debugHUD != null) debugHUD.SetActive(false);
        if (scoreCanvas != null) scoreCanvas.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void Resume()
    {
        PauseMenuUI.Instance.Hide();
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

    public TextMeshProUGUI EnableNPCDialogue()
    {
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 0;
        NPC_Dialogue.SetActive(true);
        SkiMovement.Instance.gameObject.SetActive(false);
        return NPC_Dialogue_text;
    }

    public void DisableNPCDialogue()
    {
        NPC_Dialogue.SetActive(false);
        SkiMovement.Instance.gameObject.SetActive(true);
        
        if (PlayerController.instance.skiing)
        {
            SkiMovement.Instance.anim.SetBool("do_ski", true);
        }

        StartCoroutine(WaitASingleFrameAndDoNothingElseExceptChangeCameraBlend());
    }

    private IEnumerator WaitASingleFrameAndDoNothingElseExceptChangeCameraBlend()
    {
        yield return null;
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend.Time = 2f;
    }

    public void OnEnableChoiceMenu(Quest q, UnityEvent acceptFunction, UnityEvent declineFunction)
    {
        //reset the choices on the choice thing first
        AcceptButton.onClick.RemoveAllListeners();
        DeclineButton.onClick.RemoveAllListeners();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        QuestTitle.text = q.title;
        QuestDesc.text = q.description;
        if(q.time)
        {
            QuestTime.text = $"Best Time: {q.best_time.ToString("F2")}";
        }else if(q.trick)
        {
            QuestTime.text = $"Best Score: {q.best_score.ToString()}";
        }
        else if(q.find)
        {
            QuestTime.text = $"FIND IT";
        }
        AcceptButton.onClick.AddListener(acceptFunction.Invoke);
        AcceptButton.onClick.AddListener(DisableChoiceMenu);
        DeclineButton.onClick.AddListener(declineFunction.Invoke);
        DeclineButton.onClick.AddListener(DisableChoiceMenu);
        choiceCanvas.SetActive(true);
    }

    public void DisableChoiceMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        choiceCanvas.SetActive(false);
    }
}