using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FallingRock : MonoBehaviour
{

    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 1f;

    [Header("linear motion")]
  public Vector3 fallDirection = Vector3.down;

    [Header("VFX")]
    public GameObject dustVFXPrefab;

    public float initialSpeed = 0f;


    public float acceleration = 15f;

   
    public bool startOnAwake = true;

    [Header("ground detection")]
    public LayerMask groundMask = ~0;  

   
    public float groundSkin = 0.02f;

  
    [Tooltip("How much energy to remove when hitting the player.")]
    public float energyDamage = 25f;

    [Tooltip("if true this rock is lethal")]
    public bool singleHit = true;


    [Tooltip("seconds to keep the rock after impact")]
    public float destroySecondsAfterImpact = 3f;

  
    private Vector3 _startPos;
    private float _t;               // time since drop
    private bool _falling;          // are we currently falling
    private bool _impacted;         // already hit the ground
    private bool _hasHitPlayer;     // already damaged the player
    private float _radius;          

   
    public float CurrentSpeed { get; private set; } 

    private void Awake()
    {
     
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

       
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = gameObject.AddComponent<SphereCollider>();
        }
        sc.isTrigger = true;

        
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

        //check for ground between current position and target
        Vector3 from = transform.position;
        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;

            //spherecast to see if we hit ground
            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit, dist + groundSkin, groundMask, QueryTriggerInteraction.Ignore))
            {
                //snap just above the ground
                transform.position = hit.point - dir * (_radius - groundSkin);
                Impact();
                return;
            }
        }

        // No ground hit yet
        transform.position = targetPos;
    }

    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;

        // 1. Spawn dust VFX at impact point
        if (dustVFXPrefab != null)
        {
            // spawn at the rock's position 
            GameObject dust = Instantiate(dustVFXPrefab, transform.position, Quaternion.identity);

            // auto destroy the dust after a few seconds so it doesn't clutter the scene
            Destroy(dust, 3f);
        }

        // 2. Play impact sound
        if (impactClip != null)
        {
            AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);
        }

        // 3. Disable rock visuals & collider so it turns into dust
        //    (rock stays in scene for a short time but is invisible / not interactive)

        // disable all meshrenderers on this rock (handles child meshes too)
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = false;
        }

        // disable colliders so it no longer hits the player or ground
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // 4.destroy the rock object after a short delay
        if (destroySecondsAfterImpact > 0f)
        {
            Destroy(gameObject, destroySecondsAfterImpact);
        }
        else
        {
            Destroy(gameObject);
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
