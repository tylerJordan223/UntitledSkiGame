using UnityEngine;

public class AirTimeTracker : MonoBehaviour
{
    [Header("Reference")]
    public SkiMovement skiMovement;

    [Header("Air Time")]
    public float currentAirTime;
    public float lastAirTime;
    public float bestAirTime;

    private bool wasGrounded = true;

    private void Update()
    {
        if (skiMovement == null) return;

        bool grounded = skiMovement.grounded;

        // Enter air
        if (wasGrounded && !grounded)
        {
            currentAirTime = 0f;
        }

        // While in air
        if (!grounded)
        {
            currentAirTime += Time.deltaTime;
        }

        // Land
        if (!wasGrounded && grounded)
        {
            lastAirTime = currentAirTime;
            if (lastAirTime > bestAirTime) bestAirTime = lastAirTime;
            currentAirTime = 0f;
        }

        wasGrounded = grounded;
    }
}
