using UnityEngine;
using UnityEngine.InputSystem;

public class TrickComboDetector : MonoBehaviour
{
    public enum TrickType
    {
        None = 0,
        Shifter = 1,
        Grab = 2,
        SpinLeft = 3,
        SpinRight = 4
    }

    [Header("References")]
    public SkiMovement skiMovement;
    public ScoreSystem scoreSystem;
    public AirTimeTracker airTimeTracker;
    public Animator trickAnimator;

    [Header("Trick Display")]
    public string currentTrickName = "";
    public float trickDisplaySeconds = 1.4f;

    [Header("Timing")]
    public float minimumAirTimeBeforeInput = 0.10f;
    public float minimumAirTimeToLandTrick = 0.18f;
    public float trickInputCooldown = 0.10f;

    [Header("Scoring")]
    public float shifterPoints = 250f;
    public float grabPoints = 300f;
    public float spinPoints = 450f;

    [Header("Debug State")]
    public TrickType activeTrick = TrickType.None;
    public bool isPerformingTrick = false;
    public bool trickUsedThisAir = false;
    public bool lastTrickLanded = false;

    private float trickExpireTime;
    private float lastTriggerTime;
    private bool wasGrounded = true;
    private float activeTrickBasePoints = 0f;
    private float fallbackAirTime = 0f;

    private static readonly int AnimIsPerformingTrick = Animator.StringToHash("IsPerformingTrick");
    private static readonly int AnimTrickType = Animator.StringToHash("TrickType");
    private static readonly int AnimTrickStart = Animator.StringToHash("TrickStart");
    private static readonly int AnimTrickLand = Animator.StringToHash("TrickLand");
    private static readonly int AnimTrickFail = Animator.StringToHash("TrickFail");

    private void Awake()
    {
        if (skiMovement == null)
            skiMovement = FindFirstObjectByType<SkiMovement>();

        if (scoreSystem == null)
            scoreSystem = FindFirstObjectByType<ScoreSystem>();

        if (airTimeTracker == null)
            airTimeTracker = FindFirstObjectByType<AirTimeTracker>();
    }

    private void Update()
    {
        if (skiMovement == null || Keyboard.current == null)
            return;

        bool grounded = skiMovement.grounded;

        if (wasGrounded && !grounded)
            BeginAir();

        if (!grounded)
            fallbackAirTime += Time.deltaTime;

        float currentAirTime = GetCurrentAirTime();

        if (!wasGrounded && grounded)
            ResolveLanding(currentAirTime);

        if (!grounded)
            HandleAirborneInput(currentAirTime);

        UpdateDisplay();

        wasGrounded = grounded;
    }

    private void BeginAir()
    {
        trickUsedThisAir = false;
        fallbackAirTime = 0f;
        activeTrick = TrickType.None;
        activeTrickBasePoints = 0f;
        isPerformingTrick = false;
        lastTrickLanded = false;
        SyncAnimatorState();
    }

    private void ResolveLanding(float currentAirTime)
    {
        if (activeTrick == TrickType.None)
        {
            isPerformingTrick = false;
            lastTrickLanded = false;
            SyncAnimatorState();
            return;
        }

        if (currentAirTime >= minimumAirTimeToLandTrick)
        {
            if (scoreSystem != null)
                scoreSystem.AddTrickScore(GetResolvedTrickName(activeTrick), activeTrickBasePoints);

            ShowMessage($"LANDED {GetResolvedTrickName(activeTrick)} +{Mathf.RoundToInt(activeTrickBasePoints)}");
            lastTrickLanded = true;

            if (trickAnimator != null)
                trickAnimator.SetTrigger(AnimTrickLand);
        }
        else
        {
            ShowMessage($"FAILED {GetResolvedTrickName(activeTrick)}");
            lastTrickLanded = false;

            if (trickAnimator != null)
                trickAnimator.SetTrigger(AnimTrickFail);
        }

        activeTrick = TrickType.None;
        activeTrickBasePoints = 0f;
        isPerformingTrick = false;
        SyncAnimatorState();
    }

    private void HandleAirborneInput(float currentAirTime)
    {
        bool timingOk = currentAirTime >= minimumAirTimeBeforeInput;
        bool cooldownOk = Time.time >= lastTriggerTime + trickInputCooldown;

        if (!timingOk || !cooldownOk || trickUsedThisAir)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            TriggerTrick(TrickType.Shifter, shifterPoints);
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerTrick(TrickType.Grab, grabPoints);
            return;
        }

        if (Keyboard.current.spaceKey.isPressed && Keyboard.current.aKey.wasPressedThisFrame)
        {
            TriggerTrick(TrickType.SpinLeft, spinPoints);
            return;
        }

        if (Keyboard.current.spaceKey.isPressed && Keyboard.current.dKey.wasPressedThisFrame)
        {
            TriggerTrick(TrickType.SpinRight, spinPoints);
        }
    }

    private void TriggerTrick(TrickType trickType, float basePoints)
    {
        activeTrick = trickType;
        activeTrickBasePoints = basePoints;
        trickUsedThisAir = true;
        lastTriggerTime = Time.time;
        isPerformingTrick = true;
        lastTrickLanded = false;

        ShowMessage($"{GetResolvedTrickName(trickType)}... LAND IT");
        SyncAnimatorState();

        if (trickAnimator != null)
            trickAnimator.SetTrigger(AnimTrickStart);
    }

    private void SyncAnimatorState()
    {
        if (trickAnimator == null)
            return;

        trickAnimator.SetBool(AnimIsPerformingTrick, isPerformingTrick);
        trickAnimator.SetInteger(AnimTrickType, (int)activeTrick);
    }

    private void ShowMessage(string message)
    {
        currentTrickName = message;
        trickExpireTime = Time.time + trickDisplaySeconds;
    }

    private void UpdateDisplay()
    {
        if (!string.IsNullOrEmpty(currentTrickName) && Time.time >= trickExpireTime)
            currentTrickName = "";
    }

    private float GetCurrentAirTime()
    {
        if (airTimeTracker != null)
            return airTimeTracker.currentAirTime;

        return skiMovement.grounded ? 0f : fallbackAirTime;
    }

    public string GetResolvedTrickName(TrickType trickType)
    {
        switch (trickType)
        {
            case TrickType.Shifter:
                return "SHIFTER";
            case TrickType.Grab:
                return "GRAB";
            case TrickType.SpinLeft:
                return "LEFT SPIN";
            case TrickType.SpinRight:
                return "RIGHT SPIN";
            default:
                return "TRICK";
        }
    }

    public TrickType GetActiveTrick()
    {
        return activeTrick;
    }

    public bool IsPerformingTrick()
    {
        return isPerformingTrick;
    }
}