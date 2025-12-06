using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FallingRock : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Header("Linear Motion")]
    public Vector3 fallDirection = Vector3.down;
    public float fallSpeed = 0.5f;   // UNITS PER SECOND, easy to tune

    [Header("VFX")]
    public GameObject dustVFXPrefab;

    [Header("Ground detection")]
    public LayerMask groundMask = ~0;
    public float groundSkin = 0.02f;

    [Header("Damage")]
    [Tooltip("How much HEALTH to remove when the rock hits near the player.")]
    public float energyDamage = 30f;

    [Tooltip("How far from the impact point the player can be and still take damage.")]
    public float damageRadius = 0.8f;

    [Tooltip("If true, only damages once.")]
    public bool singleHit = true;

    [Tooltip("Seconds to keep the rock after impact.")]
    public float destroySecondsAfterImpact = 0.8f;

    private bool _falling;
    private bool _impacted;
    private bool _hasHitPlayer;
    private float _radius;

    private void Awake()
    {
        // Ensure direction is valid
        if (fallDirection.sqrMagnitude < 1e-5f)
            fallDirection = Vector3.down;
        fallDirection.Normalize();

        // Ensure we have a trigger sphere collider
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;

        float maxScale = Mathf.Max(transform.lossyScale.x,
                           Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
        _radius = sc.radius * maxScale;
        if (_radius <= 0f) _radius = 0.5f;
    }

    private void Start()
    {
        // Rock starts idle; RockTriggerZone will call Drop()
        _falling = false;
        _impacted = false;
        _hasHitPlayer = false;
    }

    public void Drop()
    {
        if (_impacted) return;
        _falling = true;
    }

    private void Update()
    {
        if (!_falling || _impacted) return;

        // Make sure direction is normalised
        Vector3 dir = fallDirection.normalized;
        if (dir.sqrMagnitude < 1e-5f)
            dir = Vector3.down;

        // How far to move this frame
        float step = fallSpeed * Time.deltaTime;   // THIS is the only speed now

        Vector3 from = transform.position;
        Vector3 to = from + dir * step;

        // Spherecast down to see if we hit ground between from -> to
        if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit,
                               step + groundSkin, groundMask,
                               QueryTriggerInteraction.Ignore))
        {
            // If we hit something that is NOT the player, treat as ground
            if (!hit.collider.CompareTag("Player"))
            {
                // Snap just above ground and impact
                transform.position = hit.point - dir * (_radius - groundSkin);
                Impact();
            }
            else
            {
                // Just move through player; damage is handled in Impact()
                transform.position = to;
            }
        }
        else
        {
            // No ground hit this frame
            transform.position = to;
        }
    }

    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;

        // DAMAGE in a radius around the impact point
        if (!_hasHitPlayer && energyDamage > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Player"))
                {
                    Debug.Log($"FallingRock: impact damage to PLAYER, radius={damageRadius}");

                    if (GameManager.Instance != null)
                        GameManager.Instance.TakeDamage(energyDamage);

                    _hasHitPlayer = true;
                    break;
                }
            }
        }

        // VFX
        if (dustVFXPrefab != null)
        {
            GameObject dust = Instantiate(dustVFXPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 3f);
        }

        // SFX
        if (impactClip != null)
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);

        // Hide mesh & colliders
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (destroySecondsAfterImpact > 0f)
            Destroy(gameObject, destroySecondsAfterImpact);
        else
            Destroy(gameObject);
    }
}
