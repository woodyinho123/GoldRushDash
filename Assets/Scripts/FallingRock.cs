using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FallingRock : MonoBehaviour
{

    [Header("Audio")]
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Header("Linear Motion")]
    [Tooltip("Direction the rock moves in world space (e.g. straight down).")]
    public Vector3 fallDirection = Vector3.down;

    [Tooltip("Initial speed (u) in m/s along fallDirection.")]
    public float initialSpeed = 0f;

    [Tooltip("Acceleration (a) in m/s^2 along fallDirection. Positive accelerates along fallDirection.")]
    public float acceleration = 15f;

    [Tooltip("Start falling automatically on Start().")]
    public bool startOnAwake = true;

    [Header("Ground Detection")]
    [Tooltip("Layers considered ground. If unsure, leave as 'Everything'.")]
    public LayerMask groundMask = ~0;  // default = everything

    [Tooltip("Extra offset so we 'hit' when we are just touching the floor.")]
    public float groundSkin = 0.02f;

    [Header("Damage")]
    [Tooltip("How much energy to remove when hitting the player.")]
    public float energyDamage = 25f;

    [Tooltip("If true, this rock is essentially lethal (set damage very high).")]
    public bool singleHit = true;

    [Header("Lifetime")]
    [Tooltip("Seconds to keep the rock after impact. 0 = never destroy.")]
    public float destroySecondsAfterImpact = 3f;

    // --- runtime state ---
    private Vector3 _startPos;
    private float _t;               // time since drop
    private bool _falling;          // are we currently falling?
    private bool _impacted;         // already hit the ground?
    private bool _hasHitPlayer;     // already damaged the player?
    private float _radius;          // sphere radius for ground check

    // For debugging / UI if you want it later
    public float CurrentSpeed { get; private set; } // v = u + a t

    private void Awake()
    {
        // Normalise direction once
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

        // Make sure our SphereCollider exists and is set to trigger
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = gameObject.AddComponent<SphereCollider>();
        }
        sc.isTrigger = true;

        // Approximate world radius
        float maxScale = Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        _radius = sc.radius * maxScale;
        if (_radius <= 0f) _radius = 0.5f;
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

        // Check for ground between current position and target
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;

            // SphereCast to see if we hit ground
            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit, dist + groundSkin, groundMask, QueryTriggerInteraction.Ignore))
            {
                // Snap just above the ground
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

        //  Play impact sound
        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);
        }

        
        if (destroySecondsAfterImpact > 0f)
        {
            Destroy(gameObject, destroySecondsAfterImpact);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHitPlayer && singleHit) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("FallingRock: hit player");

            if (GameManager.Instance != null && energyDamage > 0f)
            {
                GameManager.Instance.SpendEnergy(energyDamage);
            }

            _hasHitPlayer = true;
        }
    }
}
