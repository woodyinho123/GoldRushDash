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

  
    private Vector3 _startPos;
    private float _t;               // time since drop
    private bool _falling;          // variable of are we currently falling
    private bool _impacted;         // already hit the ground
    private float _radius;          // sphere radius for ground

    public float CurrentSpeed { get; private set; } // v = u + a t

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

       
        float displacement = initialSpeed * _t + 0.5f * acceleration * _t * _t;

        Vector3 targetPos = _startPos + fallDirection * displacement;

        //  check if we hit ground between current and target
        Vector3 from = transform.position;
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

        
        SendMessage("OnRockImpact", SendMessageOptions.DontRequireReceiver);

        // clean up rock after impact
        if (destroySecondsAfterImpact > 0f)
            Destroy(gameObject, destroySecondsAfterImpact);
    }
}
