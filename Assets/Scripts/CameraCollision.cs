using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("Target (usually PlayerRoot or Focal Point)")]
    public Transform target;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;  // what counts as walls (default = everything)
    public float skinOffset = 0.2f;       // how far from the wall to keep the camera
    public float heightOffset = 1.5f;     // ray origin above target position
    public float moveSpeed = 15f;         // how fast the camera moves

    private Vector3 _defaultLocalPos;

    void Start()
    {
        // Remember where the camera is relative to its parent at start
        _defaultLocalPos = transform.localPosition;

        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Ideal camera world position (no collision)
        Vector3 desiredWorldPos = target.TransformPoint(_defaultLocalPos);

        // Ray origin: from player, slightly above feet
        Vector3 origin = target.position + Vector3.up * heightOffset;

        RaycastHit hit;
        bool hasHit = Physics.Linecast(
            origin,
            desiredWorldPos,
            out hit,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        Vector3 targetPos = desiredWorldPos;

        if (hasHit)
        {
            // Move camera to just in front of the wall
            float dist = Vector3.Distance(origin, hit.point) - skinOffset;
            dist = Mathf.Max(0.0f, dist);

            Vector3 dir = (desiredWorldPos - origin).normalized;
            targetPos = origin + dir * dist;
        }

        // Smoothly move camera to new position
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * moveSpeed
        );
    }
}
