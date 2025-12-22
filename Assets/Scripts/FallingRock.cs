using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FallingRock : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Header("Linear Motion")]
    public Vector3 fallDirection = Vector3.down;
    public float fallSpeed = 0.5f;   // UNITS PER SECOND

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

    private void Awake()//MATHS CONTENT PRESENT HERE
    {
        //  directionsure is valid
        if (fallDirection.sqrMagnitude < 1e-5f)
            fallDirection = Vector3.down;
        fallDirection.Normalize();

        // ensure we have a trigger collider
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
        // rock starts idle rockTriggerzone will call drop
        _falling = false;
        _impacted = false;
        _hasHitPlayer = false;
    }

    public void Drop()
    {
        if (_impacted) return;
        _falling = true;
    }
    //MATHS CONTENT PRESENT HERE
    private void Update()
    {
        if (!_falling || _impacted) return;

        // make sure direction is normalised**
        Vector3 dir = fallDirection.normalized;
        if (dir.sqrMagnitude < 1e-5f)
            dir = Vector3.down;

        // how far to move this frame
        float step = fallSpeed * Time.deltaTime;   // THIS is the only speed now*

        Vector3 from = transform.position;
        Vector3 to = from + dir * step;

        // spherecast  to see if we hit ground between from  to
        if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit,
                               step + groundSkin, groundMask,
                               QueryTriggerInteraction.Ignore))
        {
            // if we hit something that is notthe player then treat as ground
            if (!hit.collider.CompareTag("Player"))
            {
                // snap just above ground 
                transform.position = hit.point - dir * (_radius - groundSkin);
                Impact();
            }
            else
            {
                // just move through player but damage is handled in impact()
                transform.position = to;
            }
        }
        else
        {
            // no ground hit this frame hmm
            transform.position = to;
        }
    }
    //MATHS CONTENT PRESENT HERE
    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;

        // making damage in a radius around the impact point
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

        // vfx
        if (dustVFXPrefab != null)
        {
            GameObject dust = Instantiate(dustVFXPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 3f);
        }

        //asfx
        if (impactClip != null)
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);

        // hide mesh + colliders
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
