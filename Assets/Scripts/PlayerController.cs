using UnityEngine;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Step / Slope")]
    public float maxSlopeAngle = 55f;
    public float stepHeight = 0.35f;      
    public float stepDown = 0.6f;         // how far we can snap down after stepping
    public LayerMask movementMask = ~0;   
    public float stepUpPerFrame = 0.12f;  


    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;       
    public float turnSpeed = 180f;     // degrees per second)
    public float moveEnergyPerSecond = 5f;
    public int oreCount = 0;
    public Animator anim;
    public float mineEnergyPerSecond = 4f;

    [SerializeField] private GameObject miningPromptUI;

    
    [Header("Input Lock")]
    [SerializeField] private bool inputEnabled = true;
    public bool InputEnabled => inputEnabled;


    private Rigidbody rb;
    //  temporarily freeze rotation on ladders to stop jitter
    private RigidbodyConstraints _constraintsBeforeLadder;

    private CapsuleCollider capsule; 
    private GoldOreMineable currentOre;    // ore we are standing next to
    private bool isMining = false;         // are we currently mining?
    private bool isRunning = false;        // are we currently running?

    [Header("Damage Feedback")]
    [SerializeField] private Renderer[] damageFlashRenderers;
    [SerializeField] private float damageFlashDuration = 0.15f;

    private MaterialPropertyBlock _damageMpb;
    private Color[] _damageOrigColors;
    private int _baseColorId;
    private int _colorId;
    private Coroutine _damageFlashCo;


    [Header("Jump")]
    public float jumpForce = 6f;          // used when jumpingoff ladders
    public float forwardJumpForce = 4f;       //  forward push when ground jumping
    private bool groundJumpRequested = false; 
    private float groundJumpV = 0f;           //  stores vertical input at jump press

    [Header("Footstep Audio")]
    public AudioSource footstepSource;     // looped walking sound

    [Header("Ladder Climbing")]
    public float ladderClimbSpeed = 3f;      // up / down speed
    public float ladderSideJumpForce = 5f;   // sideways push when jumping off
    public float ladderSideMoveSpeed = 2f;

    [Header("Ladder Tuning (optional)")]
    public float ladderSpeedMultiplier = 1f;  // default 1 = unchanged

    private bool isOnLadder = false;
    private Transform currentLadder;         // which ladder we're on
    private BoxCollider currentLadderCollider; // ladder bounds for clamping
    private bool ladderJumpRequested = false;

    private bool isClimbing => isOnLadder;
    public bool IsOnLadder => isOnLadder;

    [Header("Ladder Jump")]
    public float postLadderNoRotateTime = 0.3f; // how long after ladder jump we block rotation
    private float _postLadderTimer = 0f;
    private bool _airborneFromLadder = false;

    [Header("Fall Damage")]
    // negative value  -18 means if we hit the ground while falling faster than -18
    public float fatalFallSpeed = -18f;
    private bool wasGrounded = false;
    private float lastVerticalSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();


        capsule = GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            Debug.LogError("PlayerController: No CapsuleCollider found on the player. Add one to PlayerRoot.");
        }

        if (anim == null)
        {
            // find the Animator on the miner child
            anim = GetComponentInChildren<Animator>();
        }

        if (anim == null)
        {
            Debug.LogError("PlayerController: Animator not found on child!");
        }

        if (miningPromptUI != null)
        {
            miningPromptUI.SetActive(false);
        }

        // ensures we have a footstep audio
        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }

        if (footstepSource != null)
        {
            // we want to control this manually
            footstepSource.playOnAwake = false;
            footstepSource.loop = true;
        }
        else
        {
            Debug.LogWarning("PlayerController: FootstepSource not assigned.");
        }

        // damage flash setup
        _damageMpb = new MaterialPropertyBlock();
        _baseColorId = Shader.PropertyToID("_BaseColor"); // lit
        _colorId = Shader.PropertyToID("_Color");         // standard

        if (damageFlashRenderers == null || damageFlashRenderers.Length == 0)
            damageFlashRenderers = GetComponentsInChildren<Renderer>();

        _damageOrigColors = new Color[damageFlashRenderers.Length];
        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            var r = damageFlashRenderers[i];
            if (r == null || r.sharedMaterial == null)
            {
                _damageOrigColors[i] = Color.white;
                continue;
            }

            if (r.sharedMaterial.HasProperty(_baseColorId))
                _damageOrigColors[i] = r.sharedMaterial.GetColor(_baseColorId);
            else if (r.sharedMaterial.HasProperty(_colorId))
                _damageOrigColors[i] = r.sharedMaterial.GetColor(_colorId);
            else
                _damageOrigColors[i] = Color.white;
        }

    }
    //MATHS CONTENT PRESENT HERE
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
        {
            // stop movement immediately
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            isRunning = false;
            isMining = false;

            if (footstepSource != null && footstepSource.isPlaying)
                footstepSource.Stop();

            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                anim.SetBool("IsRunning", false);
                anim.SetBool("IsClimbing", false);
                anim.SetBool("IsMining", false);
            }
        }
    }

    // PHYSICS MOVEMENT //MATHS CONTENT PRESENT HERE
    void FixedUpdate()
    {
       
        if (!inputEnabled)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            if (footstepSource != null && footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        float h = Input.GetAxisRaw("Horizontal"); // A/D + arrow keys  for turning 
        float v = Input.GetAxisRaw("Vertical");   // W/S + up/down arrow 

        // q/r strafe swapped to fix opposite directions
        float strafe = 0f;

        // q = right
        if (Input.GetKey(KeyCode.Q)) strafe += 1f;

        // r = left 
        if (Input.GetKey(KeyCode.R)) strafe -= 1f;
        //**Fix camera contantly rotating when colliding with objects***
        //  prevents collision torque from spinning the player + fixes camera bug
        rb.angularVelocity = Vector3.zero;

        // *count down no rotation window after ladder jumps
        if (_postLadderTimer > 0f)
        {
            _postLadderTimer -= Time.fixedDeltaTime;
        }

        // LADDER MODE
        if (isOnLadder)
        {
            //  ladder sideways movement uses a/d only (not arrow keys)
            float ladderH = 0f;
            if (Input.GetKey(KeyCode.A)) ladderH -= 1f;
            if (Input.GetKey(KeyCode.D)) ladderH += 1f;

            HandleLadderMovement(ladderH, v);
            // we still want fall tracking below, so do not return before that
        }

        else
        {
            // NORMAL GROUND MOVEMENT
            if (!isMining)
            {
                
                // decide if we want to run- hold left shift while moving
                bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && (Mathf.Abs(v) > 0.1f || Mathf.Abs(strafe) > 0.1f);




                // no sprinting if were out of energy
                // no sprinting if sprint is locked-only unlocks at full energy
                
                if (GameManager.Instance != null && !GameManager.Instance.CanSprint)
                {
                    wantsToRun = false;
                }


                isRunning = wantsToRun;


                float currentSpeed = isRunning ? runSpeed : walkSpeed;

                // move forward/back along local forward
                // move forward/back + strafe, but only if not in ladder airborne state
                bool canUseGroundControls = !_airborneFromLadder;

                if (canUseGroundControls)
                {
                    Vector3 moveDir = (transform.forward * -v) + (transform.right * strafe);

                    // prevents faster diagonal speed
                    if (moveDir.sqrMagnitude > 1f)
                        moveDir.Normalize();

                    Vector3 desiredDelta = moveDir * currentSpeed * Time.fixedDeltaTime;

                    if (desiredDelta.sqrMagnitude > 0.000001f)

                    {
                        Vector3 newPos = rb.position;

                        if (capsule != null)
                        {
                            // builds capsule in world 
                            float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                            float height = capsule.height * transform.lossyScale.y;
                            radius = Mathf.Max(0.01f, radius);

                            Vector3 center = transform.TransformPoint(capsule.center);

                            float half = Mathf.Max(0f, (height * 0.5f) - radius);

                            Vector3 p1 = center + Vector3.up * half;
                            Vector3 p2 = center - Vector3.up * half;

                            Vector3 dir = desiredDelta.normalized;
                            float dist = desiredDelta.magnitude;
                            const float skin = 0.02f;
                            //*fixing player running inside of walls**
                            // cast the capsule forward, if hit, stop just before the wall
                            float slopeCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

                            if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist + skin, movementMask, QueryTriggerInteraction.Ignore))

                            {
                                bool walkableSlope = hit.normal.y >= slopeCos;

                                if (walkableSlope)
                                {
                                    // move along the slope instead of stopping
                                    Vector3 slopeDelta = Vector3.ProjectOnPlane(desiredDelta, hit.normal);

                                    if (slopeDelta.sqrMagnitude > 0.000001f)
                                    {
                                        Vector3 slopeDir = slopeDelta.normalized;
                                        float slopeDist = slopeDelta.magnitude;

                                        // cast again along the slope movement to avoid clipping into other walls etc
                                        if (Physics.CapsuleCast(p1, p2, radius, slopeDir, out RaycastHit slopeHit, slopeDist + skin, movementMask, QueryTriggerInteraction.Ignore))
                                        {
                                            // If slope move is blocked by a small mud mound edge, try stepping up
                                            bool stepped = TryStepMove(ref newPos, slopeDelta, p1, p2, radius, movementMask, skin);

                                            if (!stepped)
                                            {
                                                // cant step, move as close as possible
                                                float safeSlopeDist = Mathf.Max(0f, slopeHit.distance - skin);
                                                newPos += slopeDir * safeSlopeDist;
                                            }
                                        }
                                        else
                                        {
                                            newPos += slopeDelta;
                                        }

                                    }
                                }
                                else
                                {
                                    // too steep- treat like a wall and stop just before it*
                                    float safeDist = Mathf.Max(0f, hit.distance - skin);
                                    newPos += dir * safeDist;
                                }
                            }
                            else
                            {
                                // no hit — move full amount
                                newPos += desiredDelta;
                            }

                        }
                        else
                        {
                            // fallback if capsule isnt set
                            newPos += desiredDelta;
                        }

                        Vector3 appliedDelta = newPos - rb.position;
                        rb.MovePosition(newPos);
                        //**need to fix sprint energy drain while standing still**
                        // drain sprint energy only if sprintingand we actually moved
                        if (isRunning && Mathf.Abs(v) > 0.01f && GameManager.Instance != null && appliedDelta.magnitude > 0.0005f)
                        {
                            GameManager.Instance.SpendEnergy(moveEnergyPerSecond * Time.fixedDeltaTime);
                        }
                    }
                }


                // Rrot diabled during post ladder cooldown +ladder-airborne
                bool canRotate = !_airborneFromLadder && (_postLadderTimer <= 0f);

                if (canRotate && Mathf.Abs(h) > 0.01f)
                {
                    float turnAmount = h * turnSpeed * Time.fixedDeltaTime;
                    Quaternion deltaRotation = Quaternion.Euler(0f, turnAmount, 0f);
                    rb.MoveRotation(rb.rotation * deltaRotation);
                }

                // apply ground jump in physics step this(does not interfere with ladder jump)
                if (groundJumpRequested && !isOnLadder && IsGrounded())
                {
                    groundJumpRequested = false;

                    // clear vertical  so jump height is consistent
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                    
                    Vector3 forward = transform.forward * -groundJumpV;
                    Vector3 jumpVec = Vector3.up * jumpForce + forward * forwardJumpForce;

                    rb.AddForce(jumpVec, ForceMode.VelocityChange);
                }


            }
            else
            {
                // while mining we are not running
                isRunning = false;
            }
        }

        // ANIMATION SPEED 
        // ANIMATION 
        if (anim != null)
        {
            float climbInput = Mathf.Abs(v);  // ladder up/down input
            float groundInput = Mathf.Clamp01(new Vector2(v, strafe).magnitude);

            // speed drives whichever locomotion mode were in, finish this line later**
            float speedParam =
                isMining ? 0f :
                isOnLadder ? climbInput :
                groundInput;

            anim.SetFloat("Speed", speedParam);

            // no running on ladders*
            anim.SetBool("IsRunning", isRunning && !isOnLadder && !isMining);
            //having issue with climbing animation**
            // only climbing while actually moving on the ladder to prevent climb-in-place
            bool isActivelyClimbing = isOnLadder && climbInput > 0.1f;
            anim.SetBool("IsClimbing", isActivelyClimbing);
        }



        // FALL DAMAGE 
        // track vertical speed
        lastVerticalSpeed = rb.linearVelocity.y;

        bool groundedNow = IsGrounded();

        // just landed this frame
        if (groundedNow && !wasGrounded)
        {
            if (lastVerticalSpeed < fatalFallSpeed)
            {
                // use reflection to call gamemanager.losegame with a custom message
                // same pattern as rockdamage instantDeath*
                if (GameManager.Instance != null)
                {
                    var gm = GameManager.Instance;
                    var method = gm.GetType().GetMethod(
                        "LoseGame",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    if (method != null)
                    {
                        method.Invoke(gm, new object[] { "You died from a fatal fall!" });
                    }
                }
            }
        }

        wasGrounded = groundedNow;

        // ff we hit the ground or another ladder were no longer in ladder-airborne state
        if (groundedNow || isOnLadder)
        {
            _airborneFromLadder = false;
        }

    }

    //  NON-PHYSICS and input //MATHS CONTENT PRESENT HERE
    void Update()
    {

        
        if (!inputEnabled)
        {
            if (footstepSource != null && footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        //  MINING
        // mining is not allowed if energy hit 0 and is still recharging (same lock as sprint)
        bool canMine = (GameManager.Instance == null) || GameManager.Instance.CanMine;
        bool wantsToMine = (currentOre != null && Input.GetKey(KeyCode.E) && canMine);

        if (currentOre != null)
        {
            if (wantsToMine)
            {
                currentOre.Mine(Time.deltaTime);

                // only drain energy while actively mining
                if (GameManager.Instance != null)
                    GameManager.Instance.SpendEnergy(mineEnergyPerSecond * Time.deltaTime);
            }
            else
            {
                // if player stops mining or mining is locked, reset orhide the progress bar
                currentOre.ResetMining();
            }
        }

        isMining = wantsToMine;



        if (anim != null && isMining)
        {
            Debug.Log("Setting IsMining TRUE on Animator: " + anim.gameObject.name);
        }

        // safety- if somehow we lost the ore, hide the prompt
        if (currentOre == null && miningPromptUI != null && miningPromptUI.activeSelf)
        {
            miningPromptUI.SetActive(false);
        }

        // drive mining animation
        if (anim != null)
        {
            anim.SetBool("IsMining", isMining);
        }

        // FOOTSTEPS
        // looped  on/off 
        float moveV = Input.GetAxisRaw("Vertical");
        float moveStrafe = 0f;
        if (Input.GetKey(KeyCode.Q)) moveStrafe -= 1f;
        if (Input.GetKey(KeyCode.R)) moveStrafe += 1f;

        bool isWalking = !isMining && !isOnLadder && (Mathf.Abs(moveV) > 0.1f || Mathf.Abs(moveStrafe) > 0.1f);


        if (footstepSource != null)
        {
            if (isWalking)
            {
                if (!footstepSource.isPlaying)
                    footstepSource.Play();
            }
            else
            {
                if (footstepSource.isPlaying)
                    footstepSource.Stop();
            }
        }

        // ground jump  does not affect ladder jump
        if (!isOnLadder && !isMining && Input.GetButtonDown("Jump") && IsGrounded())
        {
            groundJumpRequested = true;
            groundJumpV = Input.GetAxisRaw("Vertical");
        }


        // capture jump input for ladders so fixedupdate doesnt miss it
        if (isOnLadder && Input.GetButtonDown("Jump"))
        {
            ladderJumpRequested = true;
        }
    }

    //  ORE TRIGGERS 
    private void OnTriggerEnter(Collider other)
    {
        GoldOreMineable ore = other.GetComponent<GoldOreMineable>();
        if (ore != null)
        {
            currentOre = ore;
            Debug.Log("PlayerController: entered ore trigger");

            if (miningPromptUI != null)
            {
                miningPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GoldOreMineable ore = other.GetComponent<GoldOreMineable>();
        if (ore != null && ore == currentOre)
        {
            currentOre.ResetMining();
            currentOre = null;
            Debug.Log("PlayerController: exited ore trigger");

            if (miningPromptUI != null)
            {
                miningPromptUI.SetActive(false);
            }
        }
    }
    //MATHS CONTENT PRESENT HERE
    // LADDER API 
    public void SetOnLadder(bool onLadder, Transform ladderTransform)
    {
        // prevent spam reenter on the same ladder from resnapping every frame
        if (onLadder && isOnLadder && currentLadder == ladderTransform)
            return;

        if (onLadder)
        {
            isOnLadder = true;
            currentLadder = ladderTransform;

            // cache the ladders boxcollider on the same object as ladderzone
            currentLadderCollider = ladderTransform.GetComponent<BoxCollider>();

            // freeze rotation while on ladder to stop  causing camerajitter*
            _constraintsBeforeLadder = rb.constraints;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // snap player inside the ladder bounds once on entry to prevent jitter
            if (currentLadderCollider != null)
            {
                Bounds b = currentLadderCollider.bounds;
                float margin = 0.10f; 

                Vector3 pos = rb.position;
                pos.x = Mathf.Clamp(pos.x, b.min.x + margin, b.max.x - margin);
                pos.z = Mathf.Clamp(pos.z, b.min.z + margin, b.max.z - margin);

                rb.position = pos;                  // snap immediately
                rb.linearVelocity = Vector3.zero;   // kill any shove
                rb.angularVelocity = Vector3.zero;  // kill any spin
            }

            isRunning = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;   // stop falling while on ladder
        }
        else
        {
            if (currentLadder == ladderTransform)
            {
                isOnLadder = false;
                currentLadder = null;
                currentLadderCollider = null;

                rb.useGravity = true; // reenable gravity

                //  restore whatever constraints we had before the ladder
                rb.constraints = _constraintsBeforeLadder;
            }
        }

    }

    public void FlashDamage()
    {
        if (_damageFlashCo != null)
            StopCoroutine(_damageFlashCo);

        _damageFlashCo = StartCoroutine(FlashDamageCo());
    }

    private System.Collections.IEnumerator FlashDamageCo()
    {
        // set red******
        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            var r = damageFlashRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_damageMpb);
            _damageMpb.SetColor(_baseColorId, Color.red);
            _damageMpb.SetColor(_colorId, Color.red);
            r.SetPropertyBlock(_damageMpb);
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // restore original
        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            var r = damageFlashRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_damageMpb);
            _damageMpb.SetColor(_baseColorId, _damageOrigColors[i]);
            _damageMpb.SetColor(_colorId, _damageOrigColors[i]);
            r.SetPropertyBlock(_damageMpb);
        }

        _damageFlashCo = null;
    }

    //MATHS CONTENT PRESENT HERE
    //  LADDER MOVEMENT rigidbody 
    private void HandleLadderMovement(float h, float v)
    {
        // v = up/down inputh = left/right input

        // we start from current position
        Vector3 targetPos = rb.position;

        
        Vector3 ladderRight = (currentLadder != null) ? currentLadder.right : transform.right;


        //  vertical climb
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 climbDir = Vector3.up * v; // +1 up -1 down
            targetPos += climbDir * (ladderClimbSpeed * ladderSpeedMultiplier) * Time.fixedDeltaTime;

        }

        // sideto side move along the ladder
        if (Mathf.Abs(h) > 0.01f)
        {
            
            Vector3 sideDir = ladderRight * -h;  // a/d relative to ladder not player rotation


            targetPos += sideDir * ladderSideMoveSpeed * Time.fixedDeltaTime;
        }

        //  weclamp inside the ladders boxcollider so we dont leave sideways
        if (currentLadderCollider != null)
        {
            Bounds b = currentLadderCollider.bounds;
            float margin = 0.05f; // small padding so we dont sit exactly on the edge

            targetPos.x = Mathf.Clamp(targetPos.x, b.min.x + margin, b.max.x - margin);
            targetPos.z = Mathf.Clamp(targetPos.z, b.min.z + margin, b.max.z - margin);
        }

        
        rb.MovePosition(targetPos);

        // ump off the ladder uses the flag we set in update
        //  jump off the ladder uses the flag we set in update
        if (ladderJumpRequested)
        {
            ladderJumpRequested = false;

            // only jump off ladder if theres a sideways key held *** being really stubborn >:(
            if (Mathf.Abs(h) > 0.01f)
            {
                isOnLadder = false;
                currentLadder = null;
                currentLadderCollider = null;
                rb.useGravity = true;
                rb.constraints = _constraintsBeforeLadder; // unfreeze rotation after ladder jump remember this****

                // stop any leftover ladder velocity before we launch
                rb.linearVelocity = Vector3.zero;

                // upward + sideways push
                Vector3 jumpDir = Vector3.up * jumpForce;
                // NEW:
                Vector3 side = ladderRight * -h * ladderSideJumpForce;


                rb.AddForce(jumpDir + side, ForceMode.VelocityChange);

                // markin that were flying due to a ladder jump
                _airborneFromLadder = true;

                // start the cooldown no rotation allowed while this is > 0
                _postLadderTimer = postLadderNoRotateTime;

                if (anim != null)
                {
                    anim.SetBool("IsClimbing", false);
                    anim.SetTrigger("Jump");
                }
            }
            // else:    no sideways input - ignore space press and stay on ladder
        }



        // climb animation while on ladder
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(v));  // how fast the legs move
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsClimbing", true);
        }
    }

    private bool TryStepMove(ref Vector3 newPos, Vector3 desiredDelta,
                         Vector3 p1, Vector3 p2, float radius,
                         LayerMask mask, float skin)
    {
        if (desiredDelta.sqrMagnitude < 0.000001f)
            return false;
        // dont stepup if were not grounded prevents popping during airtime
        if (!IsGrounded())
            return false;


        Vector3 dir = desiredDelta.normalized;
        float dist = desiredDelta.magnitude;

        // try moving from a raised capsule
        Vector3 up = Vector3.up * stepHeight;
        Vector3 p1Up = p1 + up;
        Vector3 p2Up = p2 + up;

        // must have space at the raised position**
        if (Physics.CheckCapsule(p1Up, p2Up, radius, mask, QueryTriggerInteraction.Ignore))
            return false;

        // must be clear to move forward from the raised position
        if (Physics.CapsuleCast(p1Up, p2Up, radius, dir, out _, dist + skin, mask, QueryTriggerInteraction.Ignore))
            return false;

        //apply the raised move
        Vector3 steppedPos = newPos + up + desiredDelta;

        // snap down to ground so we dont float
        Vector3 snapStart = steppedPos + Vector3.up * skin;
        if (Physics.Raycast(snapStart, Vector3.down, out RaycastHit downHit, stepHeight + stepDown, mask, QueryTriggerInteraction.Ignore))
        {
            // only snap onto walkable surfaces
            float slopeCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
            if (downHit.normal.y >= slopeCos)
            {
                float targetY = downHit.point.y + skin;

                // smooth the vertical snap so mounds dont look like a hop
                steppedPos.y = Mathf.MoveTowards(newPos.y, targetY, stepUpPerFrame);

                newPos = steppedPos;
                return true;

            }
        }

        // uf no ground found still allow the step move
        newPos = steppedPos;
        return true;
    }


    //  GROUND CHECK //MATHS CONTENT PRESENT HERE
    private bool IsGrounded()
    {
        //  raycast down from  player
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
