using UnityEngine;

public class PlayerCollisionSensor : MonoBehaviour
{
    [Tooltip("Assign layers that should count as bad collision here. If left empty, tagged Obstacle objects still count.")]
    public LayerMask obstacleLayers;

    public System.Action<Collider> OnObstacleHit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        bool matchesLayer = false;

        if (obstacleLayers.value != 0)
        {
            int otherLayerMask = 1 << other.gameObject.layer;
            matchesLayer = (obstacleLayers.value & otherLayerMask) != 0;
        }

        bool matchesTag = other.CompareTag("Obstacle");

        if (!matchesLayer && !matchesTag) return;

        OnObstacleHit?.Invoke(other);
    }
}