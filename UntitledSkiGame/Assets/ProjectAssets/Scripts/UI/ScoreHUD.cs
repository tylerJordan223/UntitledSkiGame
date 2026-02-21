using TMPro;
using UnityEngine;

public class ScoreHUD : MonoBehaviour
{
    // Throw these in the inspector on the ScoreBadge object if ya'll dont have it setup alr.
    public ScoreSystem scoreSystem;
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public RectTransform badgeRoot;

    // Basic display options
    public string scorePrefix = "Score";
    public bool showChain = true; // if true: show total + current chain, else: only total

    // Pulse tuning
    public float pulseScale = 1.10f;     // peak scale for the badge
    public float kickDecay = 8f;         // how fast the kick returns to 0
    public float followSmooth = 12f;     // score number smoothing

    // While chaining, pulse on a timer so it feels alive.
    public float basePulseRate = 1.6f;   
    public float maxPulseRate = 3.2f;    
    public float chainActiveThreshold = 0.5f;

    private float visualScore;
    private float lastDisplayed;
    private float kick;        
    private float pulsePhase;  

    void Reset()
    {
        
        badgeRoot = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (scoreSystem == null || scoreText == null || comboText == null) return;

        float chain = scoreSystem.chainScore;
        float total = scoreSystem.totalScore;
        float displayed = showChain ? (total + chain) : total;

        // Smooth the counter so it doesn't look jittery.
        visualScore = Mathf.Lerp(visualScore, displayed, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));

        float combo = scoreSystem.combo;
        bool isChaining = chain > chainActiveThreshold;

        // If the displayed score jumps (usually tricks), add a stronger pulse kick.
        float deltaDisplayed = Mathf.Abs(displayed - lastDisplayed);
        if (deltaDisplayed > 1f)
        {
            float strength = Mathf.Clamp01(deltaDisplayed / 80f);
            kick = Mathf.Max(kick, strength);
        }

        if (isChaining)
        {
            // Pulse rate ramps with combo.
            float combo01 = Mathf.InverseLerp(1f, scoreSystem.comboMax, combo);
            float rate = Mathf.Lerp(basePulseRate, maxPulseRate, combo01);

            pulsePhase += Time.deltaTime * rate;
            if (pulsePhase >= 1f) pulsePhase -= 1f;

            // sharp "pop" at the start of each pulse cycle.
            float periodicPop = Mathf.Exp(-10f * pulsePhase);

            // Blend periodic pop with the kick impulse
            float pulseAmount = Mathf.Clamp01(periodicPop * 0.75f + kick);

            if (badgeRoot != null)
            {
                float scale = Mathf.Lerp(1f, pulseScale, pulseAmount);
                badgeRoot.localScale = Vector3.one * scale;
            }
        }
        else
        {
            // When not chaining, ease back to normal size.
            if (badgeRoot != null)
            {
                float t = 1f - Mathf.Exp(-kickDecay * Time.deltaTime);
                badgeRoot.localScale = Vector3.Lerp(badgeRoot.localScale, Vector3.one, t);
            }

            pulsePhase = 0f;
        }

        // Let kick fade out over time.
        kick = Mathf.MoveTowards(kick, 0f, Time.deltaTime * (kickDecay * 0.6f));

        // Text output
        int rounded = Mathf.FloorToInt(visualScore);
        scoreText.text = $"{scorePrefix}\n{rounded:N0}";

        if (combo <= 1.01f) comboText.text = "";
        else comboText.text = $"{combo:0.0}x";

        lastDisplayed = displayed;
    }
}