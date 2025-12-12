using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [Header("References")]
    public Transform pivot; // drag Focal Point here

    [Header("Collision settings")]
    public float cameraRadius = 0.25f;
    public float minDistance = 0.7f;
    public float collisionOffset = 0.15f;
    public float smoothing = 10f;

    [Header("Layers")]
    public LayerMask collisionMask = ~0; // everything by default

    private Vector3 _defaultLocalPos;
    private float _currentDist;
    private float _desiredDist;

    void Awake()
    {
        if (pivot == null)
        {
            Debug.LogWarning("CameraCollision: Pivot not assigned. Disabling.");
            enabled = false;
            return;
        }

        // Camera's intended local offset relative to pivot (Focal Point)
        _defaultLocalPos = pivot.InverseTransformPoint(transform.position);

        _currentDist = _defaultLocalPos.magnitude;
        _desiredDist = _currentDist;
    }

    void LateUpdate()
    {
        if (pivot == null) return;

        Vector3 pivotWorldPos = pivot.position;

        // Desired camera world position if there was no collision
        Vector3 desiredWorldPos = pivot.TransformPoint(_defaultLocalPos);

        Vector3 toCam = desiredWorldPos - pivotWorldPos;
        float maxDist = toCam.magnitude;
        if (maxDist < 0.001f) return;

        Vector3 dir = toCam / maxDist;

        float targetDist = maxDist;

        // SphereCast from pivot toward desired camera position
        if (Physics.SphereCast(
                pivotWorldPos,
                cameraRadius,
                dir,
                out RaycastHit hit,
                maxDist,
                collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Max(minDistance, hit.distance - collisionOffset);
        }

        // Smooth distance so it doesn't snap forward
        _desiredDist = targetDist;
        _currentDist = Mathf.Lerp(_currentDist, _desiredDist, Time.deltaTime * smoothing);

        Vector3 targetWorldPos = pivotWorldPos + dir * _currentDist;

        // Smooth position
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * smoothing);
    }
}
