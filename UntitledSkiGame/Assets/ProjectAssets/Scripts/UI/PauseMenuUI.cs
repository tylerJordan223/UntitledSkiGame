using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    [Header("Core")]
    public CanvasGroup canvasGroup;
    public Button resumeButton, restartButton, mainMenuButton, quitButton;

    [Header("Animate this — the inner panel with buttons")]
    public RectTransform menuPanel; // drag MenuContainer here

    void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        resumeButton?.onClick.AddListener(() => GameManager.instance.Resume());
        restartButton?.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        mainMenuButton?.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });
        quitButton?.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        AddHoverToAll();
        SpawnParticles();
    }

    // Show / Hide 

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        // Panel starts below screen, slides up
        if (menuPanel != null)
            menuPanel.anchoredPosition = new Vector2(0, -300f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 6f;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); 
            canvasGroup.alpha = ease;
            if (menuPanel != null)
                menuPanel.anchoredPosition = Vector2.Lerp(new Vector2(0, -300f), Vector2.zero, ease);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        if (menuPanel != null) menuPanel.anchoredPosition = Vector2.zero;
    }

    IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 7f;
            canvasGroup.alpha = Mathf.Clamp01(1f - t);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    //  Hover Effects

    void AddHoverToAll()
    {
        AddHover(resumeButton);
        AddHover(restartButton);
        AddHover(mainMenuButton);
        AddHover(quitButton);
    }

    void AddHover(Button btn)
    {
        if (btn == null) return;
        var trigger = btn.gameObject.AddComponent<EventTrigger>();

        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => StartCoroutine(ScaleBtn(btn.transform, 1.1f)));
        trigger.triggers.Add(enter);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => StartCoroutine(ScaleBtn(btn.transform, 1f)));
        trigger.triggers.Add(exit);
    }

    IEnumerator ScaleBtn(Transform t, float target)
    {
        Vector3 start = t.localScale;
        Vector3 end = Vector3.one * target;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localScale = Vector3.Lerp(start, end, elapsed / 0.1f);
            yield return null;
        }
        t.localScale = end;
    }

    // Streak Particles 

    const int STREAK_COUNT = 25;
    List<RectTransform> _streaks = new();
    List<Vector2> _vels = new();

    void SpawnParticles()
    {
        for (int i = 0; i < STREAK_COUNT; i++)
        {
            var go = new GameObject("Streak");
            go.transform.SetParent(transform, false);
            go.transform.SetSiblingIndex(0);

            var rt = go.AddComponent<RectTransform>();
            float w = Random.Range(2f, 5f);
            float h = Random.Range(20f, 60f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = RandomPos();

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, Random.Range(0.08f, 0.25f));

            // Random diagonal angle
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(15f, 35f));

            _streaks.Add(rt);
            float spd = Random.Range(150f, 350f);
            _vels.Add(new Vector2(-spd * 0.4f, -spd));
        }
    }

    void Update()
    {
        if (canvasGroup.alpha < 0.01f) return;
        float sw = Screen.width, sh = Screen.height;
        for (int i = 0; i < _streaks.Count; i++)
        {
            var pos = _streaks[i].anchoredPosition + _vels[i] * Time.unscaledDeltaTime;
            if (pos.y < -sh * 0.5f || pos.x < -sw * 0.5f)
                pos = new Vector2(Random.Range(-sw * 0.5f, sw * 0.5f), sh * 0.5f + 60f);
            _streaks[i].anchoredPosition = pos;
        }
    }

    Vector2 RandomPos()
    {
        return new Vector2(
            Random.Range(-Screen.width * 0.5f, Screen.width * 0.5f),
            Random.Range(-Screen.height * 0.5f, Screen.height * 0.5f));
    }
}