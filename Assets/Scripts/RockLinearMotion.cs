using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RockLinearMotion : MonoBehaviour
{
    //direction rock moves 
    public Vector3 fallDirection = Vector3.down;

    //initial speed
    public float initialSpeed = 0f;

    //acceleration
    public float acceleration = 15f;

    [Tooltip("Start falling automatically on Start().")]
    public bool startOnAwake = true;

    //ground detect**
    public LayerMask groundMask;

    //extra offset so hit happens just at floor
    public float groundSkin = 0.02f;

    //get rid of rock after impact
    public float destroySecondsAfterImpact = 3f;

         [Header("Impact VFX")]
     public ParticleSystem smokePoofPrefab;      // assign 
    public float smokePoofExtraLifetime = 0.5f; // safety buffer before destroying

     private Coroutine _destroyRoutine;



    [Header("Warning Indicator")]
     public bool showWarningCircle = true;
     public GameObject warningCirclePrefab;     // A
     public float warningGroundOffset = 0.02f;  // stops z-fighting
     public float warningRayDistance = 50f;     // how far down to search for ground
     public float warningRadiusMultiplier = 1f; // 1 = match spherecast radius************

     private GameObject _warningInstance;



    private Vector3 _startPos;
    private float _t;               // time since drop
    private bool _falling;          // variable of are we currently falling
    private bool _impacted;         // already hit the ground
    private float _radius;          // sphere radius for ground

    public float CurrentSpeed { get; private set; } // v = u + a t?

     [Header("Warning Fallback")]
     public float fallbackGroundY = 0f; // if raycast doesnt hit anything

    //MATHS CONTENT PRESENT HERE
    private void Awake()
    {
        // direction
        if (fallDirection.sqrMagnitude < 1e-5f) fallDirection = Vector3.down;
        fallDirection.Normalize();

        
        // sphere collider here
        var sc = GetComponent<SphereCollider>();
        if (sc != null) sc.isTrigger = true;
        _radius = sc != null ? sc.radius * Mathf.Max(transform.localScale.x, Mathf.Max(transform.localScale.y, transform.localScale.z)) : 0.5f;
    }

    private void Start()
    {
        _startPos = transform.position;
        if (startOnAwake) Drop();
    }
    //MATHS CONTENT PRESENT HERE
    public void Drop()
     {
         if (_impacted) return;
         _t = 0f;
         _falling = true;

         //  warning circle once - OBSOLETE**
         if (showWarningCircle && warningCirclePrefab != null && _warningInstance == null)
         {
             _warningInstance = Instantiate(warningCirclePrefab);

            // scale radius  diameter = radius * 2)
             float diameter = Mathf.Max(0.01f, (_radius * warningRadiusMultiplier) * 2f);
             _warningInstance.transform.localScale = new Vector3(diameter, diameter, diameter);
         }
     }

    //MATHS CONTENT PRESENT HERE
    private void Update()
    {
        if (!_falling || _impacted) return;

        _t += Time.deltaTime;

        //**v = u + a t***  math lecture notes
        CurrentSpeed = initialSpeed + acceleration * _t;

       
        float displacement = initialSpeed * _t + 0.5f * acceleration * _t * _t;

        Vector3 targetPos = _startPos + fallDirection * displacement;

        //  check if we hit ground between current and target
        Vector3 from = transform.position;
                 //  warning circle pinned to the ground beneath the rock
         if (_warningInstance != null)
                     {
                         if (Physics.Raycast(from, Vector3.down, out RaycastHit groundHit, warningRayDistance, groundMask, QueryTriggerInteraction.Ignore))
                             {
                                 // place under the rock and keep perfectly flat
                 _warningInstance.transform.position = new Vector3(from.x, groundHit.point.y + warningGroundOffset, from.z);
                                _warningInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                             }

            else
            {
                // fallback still show it under the rock  never disappears
                Vector3 p = _warningInstance.transform.position;
                p.x = from.x;
                p.z = from.z;
                p.y = fallbackGroundY + warningGroundOffset;
                _warningInstance.transform.position = p;
                _warningInstance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            }

        }

        Vector3 to = targetPos;
        Vector3 dir = (to - from);
        float dist = dir.magnitude;

        if (dist > 0f)
        {
            dir /= dist;
            
            if (Physics.SphereCast(from, _radius, dir, out RaycastHit hit, dist + groundSkin, groundMask, QueryTriggerInteraction.Ignore))
            {
                // snap to just above ground contact point
                transform.position = hit.point - dir * (_radius - groundSkin);
                Impact();
                return;
            }
        }

        // no ground hit yet*
        transform.position = targetPos;
    }

    private void Impact()
    {
        if (_impacted) return;
        _impacted = true;
        _falling = false;
        // remove warning circle (**circles white background wont tur ninvisible**) bug
        if (_warningInstance != null)
        {
            Destroy(_warningInstance);
            _warningInstance = null;
        }



        SendMessage("OnRockImpact", SendMessageOptions.DontRequireReceiver);

        // clean up rock after impact- spawn smoke when it disappears
        if (_destroyRoutine != null) StopCoroutine(_destroyRoutine);

        if (destroySecondsAfterImpact > 0f)
        {
            _destroyRoutine = StartCoroutine(DestroyAfterDelayWithSmoke(destroySecondsAfterImpact));
        }
        else
        {
            SpawnSmokePoof();
            Destroy(gameObject);
        }
    }
    private System.Collections.IEnumerator DestroyAfterDelayWithSmoke(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnSmokePoof();
        Destroy(gameObject);
    }
    //MATHS CONTENT PRESENT HERE
    private void SpawnSmokePoof()
    {
        if (smokePoofPrefab == null) return;

        ParticleSystem ps = Instantiate(smokePoofPrefab, transform.position, Quaternion.identity);

        // destroy the spawned vfx after it finishes
        float lifetime = ps.main.duration;

        // add the max start lifetime if available 
        lifetime += ps.main.startLifetime.constantMax;

        Destroy(ps.gameObject, lifetime + smokePoofExtraLifetime);
    }


    private void OnDestroy()
     {
         if (_warningInstance != null)
             Destroy(_warningInstance);
     }

}
