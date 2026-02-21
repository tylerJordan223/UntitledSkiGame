using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    // References (assign if you want; otherwise it will try to auto-find)
    public Rigidbody playerRb;
    public PlayerCollisionSensor collisionSensor;

    // Some movement setups don’t produce a reliable RB velocity (eg: moving parent transform, CC, etc).
    // Using position delta so the scoring consistent no matter how movement is implemented.
    public bool usePositionDeltaForSpeed = true;
    public Transform speedSource;
    public float positionDeltaSpeedEpsilon = 0.15f; // filters tiny jitter so idle doesn’t look like movement

    // If the player is basically not moving for long enough, wipe score back to 0.
    public bool resetScoreWhenIdle = true;
    public float idleResetSeconds = 10f;
    public float idleSpeedThreshold = 0.25f;

    // Core scoring rates
    public float minSpeedToScore = 6f;
    public float pointsPerSecondAtMinSpeed = 20f;
    public float pointsPerSecondAtHighSpeed = 80f;
    public float highSpeed = 25f;

    // Forza-ish combo: builds while clean, decays when you stop scoring
    public float combo = 1f;
    public float comboMax = 10f;
    public float comboBuildPerSecond = 0.20f;
    public float comboDecayPerSecondWhenSlow = 0.35f;

    // We “bank” the chain after the player stops scoring for a short time.
    public float bankDelayAfterLastScore = 2.0f;
    private float bankTimer;

    // chainScore = current run that’s still building, totalScore = banked runs
    public float chainScore;
    public float totalScore;

    // Trigger sensors can fire multiple times in quick succession, so add a small cooldown.
    public float collidedCooldown = 0.2f;
    private bool collidedRecently;
    private float collidedTimer;

    private float idleTimer;

    // Position-delta speed tracking
    private Vector3 lastSpeedPos;
    private bool speedPosInitialized;

    void Awake()
    {
        // Try to auto-grab references so this works even if someone forgets to wire it.
        if (playerRb == null)
        {
            playerRb = GetComponent<Rigidbody>();
            if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();
        }

        if (collisionSensor == null)
        {
            collisionSensor = GetComponentInChildren<PlayerCollisionSensor>(true);
        }

        // Default speed source to wherever this script sits.
        // If movement happens somewhere else (eg: Body child), pls dont forget to assign speedSource in inspector.
        if (speedSource == null)
        {
            speedSource = transform;
        }

        lastSpeedPos = speedSource.position;
        speedPosInitialized = true;
    }

    void OnEnable()
    {
        // Hook the sensor so chain breaks on obstacle contact.
        if (collisionSensor != null)
            collisionSensor.OnObstacleHit += HandleObstacleHit;
    }

    void OnDisable()
    {
        if (collisionSensor != null)
            collisionSensor.OnObstacleHit -= HandleObstacleHit;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Collision cooldown so one obstacle doesn’t spam break logic every frame.
        if (collidedRecently)
        {
            collidedTimer -= dt;
            if (collidedTimer <= 0f) collidedRecently = false;
        }

        float speed = GetSpeed(dt);

        // Idle reset: wipe everything if they’ve basically stopped for long enough.
        if (resetScoreWhenIdle)
        {
            if (speed <= idleSpeedThreshold)
            {
                idleTimer += dt;
                if (idleTimer >= idleResetSeconds)
                {
                    ResetAllScores();
                    idleTimer = 0f; // prevent resetting every frame
                }
            }
            else
            {
                idleTimer = 0f;
            }
        }

        bool canScore = !collidedRecently && speed >= minSpeedToScore;

        if (canScore)
        {
            // Scale points/sec based on speed
            float speed01 = Mathf.InverseLerp(minSpeedToScore, highSpeed, speed);
            float pps = Mathf.Lerp(pointsPerSecondAtMinSpeed, pointsPerSecondAtHighSpeed, speed01);

            // Build combo
            combo = Mathf.Min(comboMax, combo + comboBuildPerSecond * dt);

            // Add score into the current chain
            chainScore += pps * dt * combo;

            // Still active, so keep the chain alive
            bankTimer = bankDelayAfterLastScore;
        }
        else
        {
            // Not scoring: let combo fall back toward 1x
            combo = Mathf.Max(1f, combo - comboDecayPerSecondWhenSlow * dt);

            // If we have a chain built up, bank it after a small delay.
            if (chainScore > 0f)
            {
                bankTimer -= dt;
                if (bankTimer <= 0f)
                {
                    BankChain();
                }
            }
        }
    }

    private float GetSpeed(float dt)
    {
        // Use position delta if enabled: consistent even if movement bypasses Rigidbody velocity.
        if (usePositionDeltaForSpeed && speedSource != null)
        {
            if (!speedPosInitialized)
            {
                lastSpeedPos = speedSource.position;
                speedPosInitialized = true;
                return 0f;
            }

            Vector3 delta = speedSource.position - lastSpeedPos;
            lastSpeedPos = speedSource.position;

            float s = delta.magnitude / Mathf.Max(dt, 0.0001f);

            // Filter tiny jitter so idle doesn’t count as movement.
            if (s < positionDeltaSpeedEpsilon) s = 0f;

            return s;
        }

        // Fallback: RB-based speed 
        if (playerRb != null)
        {
            return playerRb.linearVelocity.magnitude;
        }

        return 0f;
    }

    private void HandleObstacleHit(Collider other)
    {
        BreakChain();
    }

    
    public void AddTrickScore(string trickName, float basePoints)
    {
        // Tricks scaling with combo
        chainScore += basePoints * combo;

        // Keep chain alive and make sure it dosen't idle-reset mid trick spam.
        bankTimer = bankDelayAfterLastScore;
        idleTimer = 0f;
    }

    public void BreakChain()
    {
        // Hard reset of the chain on collision.
        chainScore = 0f;
        combo = 1f;
        bankTimer = 0f;

        collidedRecently = true;
        collidedTimer = collidedCooldown;
    }

    public void BankChain()
    {
        totalScore += chainScore;
        chainScore = 0f;
        combo = 1f;
        bankTimer = 0f;
    }

    private void ResetAllScores()
    {
        chainScore = 0f;
        totalScore = 0f;
        combo = 1f;
        bankTimer = 0f;

        // Clear collision gating too so it doesn’t feel “stuck” after an idle reset.
        collidedRecently = false;
        collidedTimer = 0f;
    }
}