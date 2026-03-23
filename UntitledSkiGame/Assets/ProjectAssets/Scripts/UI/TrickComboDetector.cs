using UnityEngine;
using UnityEngine.InputSystem;

public class TrickComboDetector : MonoBehaviour
{
    [Header("Reference")]
    public SkiMovement skiMovement;

    [Header("Trick Display")]
    public string currentTrickName = "";
    public float trickDisplaySeconds = 1.0f;

    private float trickExpireTime = 0f;

    private void Update()
    {
        if (skiMovement == null) return;
        if (Keyboard.current == null) return;

        bool canTriggerRampTrick = !skiMovement.grounded && skiMovement.canDoRampTricks;

        if (canTriggerRampTrick && Keyboard.current.qKey.wasPressedThisFrame)
            TriggerTrick("TRICK: SHIFTER (placeholder)");

        if (canTriggerRampTrick && Keyboard.current.eKey.wasPressedThisFrame)
            TriggerTrick("TRICK: GRAB (placeholder)");

        if (canTriggerRampTrick && Keyboard.current.spaceKey.isPressed && Keyboard.current.aKey.wasPressedThisFrame)
            TriggerTrick("TRICK: SPIN LEFT (placeholder)");

        if (canTriggerRampTrick && Keyboard.current.spaceKey.isPressed && Keyboard.current.dKey.wasPressedThisFrame)
            TriggerTrick("TRICK: SPIN RIGHT (placeholder)");

        // Expire text
        if (!string.IsNullOrEmpty(currentTrickName) && Time.time >= trickExpireTime)
        {
            currentTrickName = "";
        }
    }

    private void TriggerTrick(string name)
    {
        currentTrickName = name;
        trickExpireTime = Time.time + trickDisplaySeconds;
    }
}