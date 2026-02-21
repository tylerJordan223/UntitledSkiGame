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

        // TEMP DEBUG: allow triggering even when grounded.
        // if (skiMovement.grounded)
        //     return;

        // Placeholder combos
        // Example mappings:
        //  - Q = "SHIFTER"
        //  - E = "GRAB"
        //  - Space + A = "SPIN LEFT"
        //  - Space + D = "SPIN RIGHT"

        if (Keyboard.current.qKey.wasPressedThisFrame)
            TriggerTrick("TRICK: SHIFTER (placeholder)");

        if (Keyboard.current.eKey.wasPressedThisFrame)
            TriggerTrick("TRICK: GRAB (placeholder)");

        if (Keyboard.current.spaceKey.isPressed && Keyboard.current.aKey.wasPressedThisFrame)
            TriggerTrick("TRICK: SPIN LEFT (placeholder)");

        if (Keyboard.current.spaceKey.isPressed && Keyboard.current.dKey.wasPressedThisFrame)
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
