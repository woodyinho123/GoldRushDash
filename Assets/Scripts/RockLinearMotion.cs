using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RockLinearMotion : MonoBehaviour
{
    [Header("Linear Motion (world space)")]
    [Tooltip("Direction the rock moves in world space (e.g. straight down).")]
    public Vector3 fallDirection = Vector3.down;

    [Tooltip("Initial speed (u) in m/s along fallDirection.")]
    public float initialSpeed = 0f;

    [Tooltip("Acceleration (a) in m/s^2 along fallDirection. Positive accelerates along fallDirection.")]
    public float acceleration = 15f;

    [Tooltip("Start falling automatically on Start().")]
    public bool startOnAwake = true;

    [Header("Ground Detection")]
    [Tooltip("Layers considered ground.")]
    public LayerMask groundMask;

    [Tooltip("Extra offset so we 'hit' when we are just touching the floor.")]
    public float groundSkin = 0.02f;

    [Header("Lifetime")]
    public float destroySecondsAfterImpact = 3f;

    // cached/runtime
    private Vector3 _startPos;
    private float _t;               // time since drop
    private bool _falling;          // are we currently falling?
    private bool _impacted;         // already hit the ground?
    private float _radius;          // sphere radius for ground check

    // Exposed read-only for HUD/debug
    public float CurrentSpeed { get; private set; } // v = u + a t

    private void Awake()
    {
        // Normalise direction once
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

        // Read the sphere collider radius for hit distance
        var sc = GetComponent<SphereCollider>();
        _radius = sc != null ? sc.radius * Mathf.Max(transform.localScale.x, Mathf.Max(transform.localScale.y, transform.localScale.z)) : 0.5f;
    }

    private void Start()
    {
        _startPos = transform.position;
        if (startOnAwake) Drop();
    }

    public void Drop()
    {
        if (_impacted) return;
        _t = 0f;
        _falling = true;
    }

    private void Update()
    {
        if (!_falling || _impacted) return;

        _t += Time.deltaTime;

        // v = u + a t
        CurrentSpeed = initialSpeed + acceleration * _t;

        // s = u t + 1/2 a t^2
        float displacement = initialSpeed * _t + 0.5f * acceleration * _t * _t;

        Vector3 targetPos = _startPos + fallDirection * displacement;

        // Before we commit, check if we'd hit ground between current and target
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;
            // Raycast forward by 'dist + radius' to see if ground is intersected
            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit, dist + groundSkin, groundMask, QueryTriggerInteraction.Ignore))
            {
                // Snap to just above ground contact point
                transform.position = hit.point - dir * (_radius - groundSkin);
                Impact();
                return;
            }
        }

        // No ground hit yet → move freely
        transform.position = targetPos;
    }

    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;

        // Try to tell any audio/VFX on the same GameObject
        SendMessage("OnRockImpact", SendMessageOptions.DontRequireReceiver);

        // Clean up later
        if (destroySecondsAfterImpact > 0f)
            Destroy(gameObject, destroySecondsAfterImpact);
    }
}
