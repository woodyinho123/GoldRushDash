using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FallingRock : MonoBehaviour
{
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Header("linear motion")]
    public Vector3 fallDirection = Vector3.down;
    public float initialSpeed = 0f;
    public float acceleration = 15f;
    public bool startOnAwake = true;

    [Header("VFX")]
    public GameObject dustVFXPrefab;

    [Header("ground detection")]
    public LayerMask groundMask = ~0;
    public float groundSkin = 0.02f;

    [Tooltip("How much HEALTH to remove when hitting the player.")]
    public float energyDamage = 25f;   // this is actually health damage now
    [Tooltip("if true this rock is lethal (only damages once)")]
    public bool singleHit = true;

    [Tooltip("seconds to keep the rock after impact")]
    public float destroySecondsAfterImpact = 3f;

    private Vector3 _startPos;
    private float _t;
    private bool _falling;
    private bool _impacted;
    private bool _hasHitPlayer;
    private float _radius;

    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        // normalise direction
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

        // ensure we have a SphereCollider used as trigger for damage + size for spherecast
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;

        // world-space radius for spherecast
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

        CurrentSpeed = initialSpeed + acceleration * _t;
        float displacement = initialSpeed * _t + 0.5f * acceleration * _t * _t;

        Vector3 targetPos = _startPos + fallDirection * displacement;

        // check for ground between current position and target
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;

            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit, dist + groundSkin, groundMask, QueryTriggerInteraction.Ignore))
            {
                // If we hit the PLAYER with the spherecast, do NOT treat it as ground – keep falling
                if (hit.collider.CompareTag("Player"))
                {
                    transform.position = targetPos;
                }
                else
                {
                    // real ground hit
                    transform.position = hit.point - dir * (_radius - groundSkin);
                    Impact();
                    return;
                }
            }
            else
            {
                // no ground hit yet
                transform.position = targetPos;
            }
        }
        else
        {
            transform.position = targetPos;
        }
    }

    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;

        // spawn dust
        if (dustVFXPrefab != null)
        {
            GameObject dust = Instantiate(dustVFXPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 3f);
        }

        // sound
        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);
        }

        // disable visuals & colliders so it looks like it turned to dust
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (destroySecondsAfterImpact > 0f)
            Destroy(gameObject, destroySecondsAfterImpact);
        else
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHitPlayer && singleHit) return;

        Debug.Log("FallingRock trigger hit: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("FallingRock: hit PLAYER, dealing damage");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TakeDamage(energyDamage);
                Debug.Log($"FallingRock: requested {energyDamage} damage from GameManager.");
            }

            _hasHitPlayer = true;
        }
    }




}
