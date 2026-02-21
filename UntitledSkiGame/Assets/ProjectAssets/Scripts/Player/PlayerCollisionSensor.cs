using UnityEngine;

public class PlayerCollisionSensor : MonoBehaviour
{
    [Tooltip("Assign layers that should count as 'bad collision' here xdd (trees, rocks, buildings).")]
    public LayerMask obstacleLayers;

    public System.Action<Collider> OnObstacleHit;

    private void OnTriggerEnter(Collider other)
    {
        // Ignore triggers
        if (other.isTrigger) return;

        // Layer check
        int otherLayerMask = 1 << other.gameObject.layer;
        if ((obstacleLayers.value & otherLayerMask) == 0) return;

        OnObstacleHit?.Invoke(other);
    }
}