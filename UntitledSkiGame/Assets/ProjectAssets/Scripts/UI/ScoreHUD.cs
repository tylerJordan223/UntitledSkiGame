using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHUD : MonoBehaviour
{
    public ScoreSystem scoreSystem;
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public RectTransform badgeRoot;

    //  background image to flash : probs image on scorebadge
    public Image badgeBackground;

    //  small text that appears briefly on chain break
    public TMP_Text breakText;

    public string scorePrefix = "STYLE";
    public bool showChain = true;

    public float pulseScale = 1.10f;
    public float kickDecay = 8f;
    public float followSmooth = 12f;

    public float basePulseRate = 1.6f;
    public float maxPulseRate = 3.2f;
    public float chainActiveThreshold = 0.5f;

    // Chain break feedback tuning
    public float breakFlashDuration = 0.25f;
    public float breakTextDuration = 0.6f;
    public Color breakFlashColor = new Color(1f, 0.25f, 0.25f, 0.85f);

    private float visualScore;
    private float lastDisplayed;
    private float kick;
    private float pulsePhase;

    private float flashTimer;
    private float breakTextTimer;
    private Color baseBgColor;

    void Reset()
    {
        badgeRoot = GetComponent<RectTransform>();
        badgeBackground = GetComponent<Image>();
    }

    void Awake()
    {
        if (badgeRoot == null) badgeRoot = GetComponent<RectTransform>();
        if (badgeBackground == null) badgeBackground = GetComponent<Image>();

        if (badgeBackground != null)
            baseBgColor = badgeBackground.color;

        if (breakText != null)
        {
            var c = breakText.color;
            breakText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void OnEnable()
    {
        if (scoreSystem != null)
            scoreSystem.ChainBroken += HandleChainBroken;
    }

    void OnDisable()
    {
        if (scoreSystem != null)
            scoreSystem.ChainBroken -= HandleChainBroken;
    }

    void HandleChainBroken()
    {
        flashTimer = breakFlashDuration;
        breakTextTimer = breakTextDuration;

        // Make the pulse kick noticeable on break too
        kick = Mathf.Max(kick, 1f);

        if (breakText != null)
            breakText.text = "CHAIN BROKE";
    }

    void Update()
    {
        if (scoreSystem == null || scoreText == null || comboText == null) return;

        float chain = scoreSystem.chainScore;
        float total = scoreSystem.totalScore;
        float displayed = showChain ? (total + chain) : total;

        visualScore = Mathf.Lerp(visualScore, displayed, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));

        float combo = scoreSystem.combo;
        bool isChaining = chain > chainActiveThreshold;

        // Kick pulse when score jumps (tricks / burst gain)
        float deltaDisplayed = Mathf.Abs(displayed - lastDisplayed);
        if (deltaDisplayed > 1f)
        {
            float strength = Mathf.Clamp01(deltaDisplayed / 80f);
            kick = Mathf.Max(kick, strength);
        }

        if (isChaining)
        {
            float combo01 = Mathf.InverseLerp(1f, scoreSystem.comboMax, combo);
            float rate = Mathf.Lerp(basePulseRate, maxPulseRate, combo01);

            pulsePhase += Time.deltaTime * rate;
            if (pulsePhase >= 1f) pulsePhase -= 1f;

            float periodicPop = Mathf.Exp(-10f * pulsePhase);
            float pulseAmount = Mathf.Clamp01(periodicPop * 0.75f + kick);

            if (badgeRoot != null)
            {
                float scale = Mathf.Lerp(1f, pulseScale, pulseAmount);
                badgeRoot.localScale = Vector3.one * scale;
            }
        }
        else
        {
            if (badgeRoot != null)
            {
                float t = 1f - Mathf.Exp(-kickDecay * Time.deltaTime);
                badgeRoot.localScale = Vector3.Lerp(badgeRoot.localScale, Vector3.one, t);
            }

            pulsePhase = 0f;
        }

        kick = Mathf.MoveTowards(kick, 0f, Time.deltaTime * (kickDecay * 0.6f));

        // Flash the badge background on chain break
        if (badgeBackground != null)
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                float a = Mathf.Clamp01(flashTimer / Mathf.Max(breakFlashDuration, 0.001f));
                badgeBackground.color = Color.Lerp(baseBgColor, breakFlashColor, a);
            }
            else
            {
                badgeBackground.color = baseBgColor;
            }
        }

        // Show "CHAIN BROKE" briefly
        if (breakText != null)
        {
            if (breakTextTimer > 0f)
            {
                breakTextTimer -= Time.deltaTime;
                float a = Mathf.Clamp01(breakTextTimer / Mathf.Max(breakTextDuration, 0.001f));
                var c = breakText.color;
                breakText.color = new Color(c.r, c.g, c.b, a);
            }
            else
            {
                var c = breakText.color;
                breakText.color = new Color(c.r, c.g, c.b, 0f);
            }
        }

        int rounded = Mathf.FloorToInt(visualScore);
        scoreText.text = $"{scorePrefix}\n{rounded:N0}";

        if (combo <= 1.01f) comboText.text = "";
        else comboText.text = $"{combo:0.0}x";

        lastDisplayed = displayed;
    }
}