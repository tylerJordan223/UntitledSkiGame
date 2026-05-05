using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public Button playButton;
    public Button quitButton;
    public string gameSceneName = "OpenWorld";

    [Header("Title (drag in a TextMeshPro text object)")]
    public TextMeshProUGUI titleText;

    // Snow settings
    const int SNOWFLAKE_COUNT = 60;
    const float SNOW_SPEED_MIN = 80f;
    const float SNOW_SPEED_MAX = 200f;
    const float SNOW_DRIFT = 30f;

    List<RectTransform> _flakes = new();
    List<Vector2> _velocities = new();
    RectTransform _canvasRect;

    void Start()
    {
        _canvasRect = canvasGroup.GetComponent<RectTransform>();

        SpawnSnow();
        SetupButtonHovers();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        StartCoroutine(FadeIn());

        playButton?.onClick.AddListener(() => StartCoroutine(FadeAndLoad()));
        quitButton?.onClick.AddListener(Quit);
    }

    void Update()
    {
        TickSnow();

        // Pulse title
        if (titleText != null)
            titleText.alpha = 0.75f + Mathf.Sin(Time.time * 1.5f) * 0.25f;
    }

    //  Snow

    void SpawnSnow()
    {
        for (int i = 0; i < SNOWFLAKE_COUNT; i++)
        {
            var go = new GameObject("Flake");
            go.transform.SetParent(canvasGroup.transform, false);
            go.transform.SetSiblingIndex(0); // behind buttons

            var rt = go.AddComponent<RectTransform>();
            float size = Random.Range(4f, 14f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = RandomOnScreen();

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, Random.Range(0.3f, 0.9f));

            _flakes.Add(rt);
            _velocities.Add(new Vector2(
                Random.Range(-SNOW_DRIFT, SNOW_DRIFT),
                -Random.Range(SNOW_SPEED_MIN, SNOW_SPEED_MAX)
            ));
        }
    }

    void TickSnow()
    {
        float w = _canvasRect.rect.width;
        float h = _canvasRect.rect.height;

        for (int i = 0; i < _flakes.Count; i++)
        {
            var pos = _flakes[i].anchoredPosition;
            pos += _velocities[i] * Time.deltaTime;

            // Drift wobble
            pos.x += Mathf.Sin(Time.time * 0.7f + i) * 0.4f;

            // Reset when off bottom
            if (pos.y < -h * 0.5f)
            {
                pos.y = h * 0.5f + 10f;
                pos.x = Random.Range(-w * 0.5f, w * 0.5f);
            }

            _flakes[i].anchoredPosition = pos;
        }
    }

    Vector2 RandomOnScreen()
    {
        float w = Screen.width;
        float h = Screen.height;
        return new Vector2(Random.Range(-w * 0.5f, w * 0.5f),
                           Random.Range(-h * 0.5f, h * 0.5f));
    }

    //  Button hovers 

    void SetupButtonHovers()
    {
        SetupHover(playButton);
        SetupHover(quitButton);
    }

    void SetupHover(Button btn)
    {
        if (btn == null) return;
        var trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enter = new UnityEngine.EventSystems.EventTrigger.Entry();
        enter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enter.callback.AddListener(_ => StartCoroutine(ScaleButton(btn.transform, 1.08f)));
        trigger.triggers.Add(enter);

        var exit = new UnityEngine.EventSystems.EventTrigger.Entry();
        exit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exit.callback.AddListener(_ => StartCoroutine(ScaleButton(btn.transform, 1f)));
        trigger.triggers.Add(exit);
    }

    IEnumerator ScaleButton(Transform t, float target)
    {
        float elapsed = 0f;
        Vector3 start = t.localScale;
        Vector3 end = Vector3.one * target;
        while (elapsed < 0.12f)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(start, end, elapsed / 0.12f);
            yield return null;
        }
        t.localScale = end;
    }

    //  Fades 

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.2f;
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    IEnumerator FadeAndLoad()
    {
        canvasGroup.interactable = false;
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 2f;
            canvasGroup.alpha = t;
            yield return null;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}