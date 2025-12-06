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
    public bool startOnAwake = false;   // we trigger by zone

    [Header("VFX")]
    public GameObject dustVFXPrefab;

    [Header("ground detection")]
    public LayerMask groundMask = ~0;
    public float groundSkin = 0.02f;

    [Tooltip("How much HEALTH to remove when hitting the player.")]
    public float energyDamage = 30f;
    [Tooltip("If true, only damages once.")]
    public bool singleHit = true;
    [Tooltip("Radius around the impact where the player takes damage.")]
    public float damageRadius = 2f;
 [Tooltip("Seconds to keep the rock after impact")]
    public float destroySecondsAfterImpact = 0.8f;

    private Vector3 _startPos;
    private float _t;
    private bool _falling;
    private bool _impacted;
    private bool _hasHitPlayer;
    private float _radius;

    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;   // trigger used for damage

        float maxScale = Mathf.Max(transform.lossyScale.x,
                           Mathf.Max(transform.lossyScale.y, transform.lossyScale.z));
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

        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;

            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit,
                                   dist + groundSkin, groundMask,
                                   QueryTriggerInteraction.Ignore))
            {
                // If we hit the player with the spherecast, ignore it as "ground"
                if (hit.collider.CompareTag("Player"))
                {
                    transform.position = targetPos;
                }
                else
                {
                    transform.position = hit.point - dir * (_radius - groundSkin);
                    Impact();
                    return;
                }
            }
            else
            {
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

        // First: damage player near the impact point
        DealImpactDamage();

        //  pllay impact sound
        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);
        }

        // spawn dust VFX
        if (dustVFXPrefab != null)
        {
            GameObject dust = Instantiate(dustVFXPrefab, transform.position, Quaternion.identity);
            Destroy(dust, 3f);
        }

        // hide rock mesh & colliders so it "turns to dust"
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            mr.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (destroySecondsAfterImpact > 0f)
            Destroy(gameObject, destroySecondsAfterImpact);
        else
            Destroy(gameObject);
    }


    private void DealImpactDamage()
    {
        if (GameManager.Instance == null || energyDamage <= 0f) return;

        // Check everything around the rock’s impact point
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            damageRadius,
            ~0,                         // all layers
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log($"Rock impact damaged player for {energyDamage}");
                GameManager.Instance.TakeDamage(energyDamage);
                break; // only damage once
            }
        }
    }

   




}
