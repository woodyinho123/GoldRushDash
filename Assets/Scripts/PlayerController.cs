using UnityEngine;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Step / Slope")]
    public float maxSlopeAngle = 55f;
    public float stepHeight = 0.35f;      // try 0.25–0.45
    public float stepDown = 0.6f;         // how far we can snap down after stepping
    public LayerMask movementMask = ~0;   // set in Inspector if needed
    public float stepUpPerFrame = 0.12f;  // NEW: limits vertical pop (tune 0.08–0.2)


    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;        // faster
    public float turnSpeed = 180f;     // degrees per second
    public float moveEnergyPerSecond = 5f;
    public int oreCount = 0;
    public Animator anim;
    public float mineEnergyPerSecond = 4f;

    [SerializeField] private GameObject miningPromptUI;
    

    private Rigidbody rb;
    private CapsuleCollider capsule; // NEW
    private GoldOreMineable currentOre;    // ore we are standing next to
    private bool isMining = false;         // are we currently mining?
    private bool isRunning = false;        // are we currently running?

    [Header("Jump")]
    public float jumpForce = 6f;          // used when jumping OFF ladders
    public float forwardJumpForce = 4f;       // NEW: forward push when ground-jumping
    private bool groundJumpRequested = false; // NEW
    private float groundJumpV = 0f;           // NEW: stores vertical input at jump press

    [Header("Footstep Audio")]
    public AudioSource footstepSource;     // looped walking sound

    [Header("Ladder Climbing")]
    public float ladderClimbSpeed = 3f;      // up / down speed
    public float ladderSideJumpForce = 5f;   // sideways push when jumping off
    public float ladderSideMoveSpeed = 2f;

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
    // Negative value: e.g. -18 means "if we hit the ground while falling faster than -18"
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
            // Find the Animator on the Miner child
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

        // Ensure we have a footstep AudioSource
        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }

        if (footstepSource != null)
        {
            // We want to control this manually
            footstepSource.playOnAwake = false;
            footstepSource.loop = true;
        }
        else
        {
            Debug.LogWarning("PlayerController: FootstepSource not assigned.");
        }
    }

    // ---------------------- PHYSICS MOVEMENT ----------------------
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // Count down "no rotation" window after ladder jumps
        if (_postLadderTimer > 0f)
        {
            _postLadderTimer -= Time.fixedDeltaTime;
        }

        // LADDER MODE
        if (isOnLadder)
        {
            HandleLadderMovement(h, v);
            // we still want fall tracking below, so do NOT 'return' before that
        }
        else
        {
            // --------- NORMAL GROUND MOVEMENT ---------
            if (!isMining)
            {
                // decide if we want to run (hold Left Shift while moving)
                // decide if we want to run (hold Left Shift while moving)
                bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(v) > 0.1f;



                // No sprinting if we're out of energy
                // No sprinting if sprint is locked (only unlocks at full energy)
                // No sprinting if sprint is locked (only unlocks at full energy)
                if (GameManager.Instance != null && !GameManager.Instance.CanSprint)
                {
                    wantsToRun = false;
                }


                isRunning = wantsToRun;


                float currentSpeed = isRunning ? runSpeed : walkSpeed;

                // move forward/back along local forward
                bool canUseGroundControls = !_airborneFromLadder;

                // move forward/back along local forward (only if not in ladder-airborne state)
                if (canUseGroundControls)
                {
                    Vector3 desiredDelta = transform.forward * -v * currentSpeed * Time.fixedDeltaTime;

                    if (desiredDelta.sqrMagnitude > 0.000001f)
                    {
                        Vector3 newPos = rb.position;

                        if (capsule != null)
                        {
                            // Build capsule endpoints in world space
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

                            // Cast the capsule forward; if hit, stop just before the wall
                            float slopeCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

                            if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist + skin, movementMask, QueryTriggerInteraction.Ignore))

                            {
                                bool walkableSlope = hit.normal.y >= slopeCos;

                                if (walkableSlope)
                                {
                                    // Move along the slope instead of stopping
                                    Vector3 slopeDelta = Vector3.ProjectOnPlane(desiredDelta, hit.normal);

                                    if (slopeDelta.sqrMagnitude > 0.000001f)
                                    {
                                        Vector3 slopeDir = slopeDelta.normalized;
                                        float slopeDist = slopeDelta.magnitude;

                                        // Cast again along the slope movement to avoid clipping into other geometry
                                        if (Physics.CapsuleCast(p1, p2, radius, slopeDir, out RaycastHit slopeHit, slopeDist + skin, movementMask, QueryTriggerInteraction.Ignore))
                                        {
                                            // If slope-move is blocked by a small lip/mound edge, try stepping up
                                            bool stepped = TryStepMove(ref newPos, slopeDelta, p1, p2, radius, movementMask, skin);

                                            if (!stepped)
                                            {
                                                // Can't step -> move as close as possible
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
                                    // Too steep: treat like a wall (stop just before it)
                                    float safeDist = Mathf.Max(0f, hit.distance - skin);
                                    newPos += dir * safeDist;
                                }
                            }
                            else
                            {
                                // No hit — move full amount
                                newPos += desiredDelta;
                            }

                        }
                        else
                        {
                            // Fallback if capsule isn't set
                            newPos += desiredDelta;
                        }

                        Vector3 appliedDelta = newPos - rb.position;
                        rb.MovePosition(newPos);

                        // Drain sprint energy only if sprinting AND we actually moved
                        if (isRunning && Mathf.Abs(v) > 0.01f && GameManager.Instance != null && appliedDelta.magnitude > 0.0005f)
                        {
                            GameManager.Instance.SpendEnergy(moveEnergyPerSecond * Time.fixedDeltaTime);
                        }
                    }
                }


                // ROTATION (turn left/right) - DISABLED during post-ladder cooldown AND ladder-airborne
                bool canRotate = !_airborneFromLadder && (_postLadderTimer <= 0f);

                if (canRotate && Mathf.Abs(h) > 0.01f)
                {
                    float turnAmount = h * turnSpeed * Time.fixedDeltaTime;
                    Quaternion deltaRotation = Quaternion.Euler(0f, turnAmount, 0f);
                    rb.MoveRotation(rb.rotation * deltaRotation);
                }

                // Apply ground jump in physics step (does not interfere with ladder jump)
                if (groundJumpRequested && !isOnLadder && IsGrounded())
                {
                    groundJumpRequested = false;

                    // Clear vertical velocity so jump height is consistent
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                    // Use your same "forward sign" convention (-v)
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

        // ANIMATION SPEED (walking) + running flag
        if (anim != null)
        {
            float speedParam = (!isMining) ? Mathf.Abs(v) : 0f;
            anim.SetFloat("Speed", speedParam);
            anim.SetBool("IsRunning", isRunning);
            anim.SetBool("IsClimbing", isOnLadder);
        }

        // ---------------- FALL DAMAGE ----------------
        // Track vertical speed
        lastVerticalSpeed = rb.linearVelocity.y;

        bool groundedNow = IsGrounded();

        // Just landed this frame
        if (groundedNow && !wasGrounded)
        {
            if (lastVerticalSpeed < fatalFallSpeed)
            {
                // Use reflection to call GameManager.LoseGame with a custom message,
                // same pattern as RockDamage instantDeath.
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

        // If we hit the ground or another ladder, we're no longer in ladder-airborne state
        if (groundedNow || isOnLadder)
        {
            _airborneFromLadder = false;
        }

    }

    // ---------------------- NON-PHYSICS / INPUT ----------------------
    void Update()
    {
        // ---------------- MINING ----------------
        bool wantsToMine = (currentOre != null && Input.GetKey(KeyCode.E));

        if (currentOre != null)
        {
            if (wantsToMine)
            {
                currentOre.Mine(Time.deltaTime);

                // ONLY drain energy while actively mining (holding E)
                if (GameManager.Instance != null)
                    GameManager.Instance.SpendEnergy(mineEnergyPerSecond * Time.deltaTime);
            }
            else
            {
                currentOre.ResetMining();
            }
        }

        isMining = wantsToMine;


        if (anim != null && isMining)
        {
            Debug.Log("Setting IsMining TRUE on Animator: " + anim.gameObject.name);
        }

        // Safety: if somehow we lost the ore, hide the prompt
        if (currentOre == null && miningPromptUI != null && miningPromptUI.activeSelf)
        {
            miningPromptUI.SetActive(false);
        }

        // Drive mining animation
        if (anim != null)
        {
            anim.SetBool("IsMining", isMining);
        }

        // ----- FOOTSTEPS (looped source on/off) -----
        float moveInput = Input.GetAxisRaw("Vertical");
        bool isWalking = !isMining && Mathf.Abs(moveInput) > 0.1f && !isOnLadder;

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

        // Ground jump (Space + forward/back). Does NOT affect ladder jump.
        if (!isOnLadder && !isMining && Input.GetButtonDown("Jump") && IsGrounded())
        {
            groundJumpRequested = true;
            groundJumpV = Input.GetAxisRaw("Vertical");
        }


        // Capture jump input for ladders (so FixedUpdate doesn't miss it)
        if (isOnLadder && Input.GetButtonDown("Jump"))
        {
            ladderJumpRequested = true;
        }
    }

    // ---------------------- ORE TRIGGERS ----------------------
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

    // ---------------------- LADDER API (called by LadderZone) ----------------------
    public void SetOnLadder(bool onLadder, Transform ladderTransform)
    {
        if (onLadder)
        {
            isOnLadder = true;
            currentLadder = ladderTransform;

            // Cache the ladder's BoxCollider (on the same object as LadderZone)
            currentLadderCollider = ladderTransform.GetComponent<BoxCollider>();

            isRunning = false;
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;   // stop falling while on ladder
        }
        else
        {
            if (currentLadder == ladderTransform)
            {
                isOnLadder = false;
                currentLadder = null;
                currentLadderCollider = null;
                rb.useGravity = true;    // re-enable gravity
            }
        }
    }

    // ---------------------- LADDER MOVEMENT (Rigidbody) ----------------------
    private void HandleLadderMovement(float h, float v)
    {
        // v = up/down input, h = left/right input

        // Start from current position
        Vector3 targetPos = rb.position;

        // 1) Vertical climb
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 climbDir = Vector3.up * v; // +1 up, -1 down
            targetPos += climbDir * ladderClimbSpeed * Time.fixedDeltaTime;
        }

        // 2) Side-to-side move along the ladder
        if (Mathf.Abs(h) > 0.01f)
        {
            Vector3 sideDir = transform.right * -h;  // A/D
            targetPos += sideDir * ladderSideMoveSpeed * Time.fixedDeltaTime;
        }

        // 3) Clamp inside the ladder's BoxCollider so we don't leave sideways
        if (currentLadderCollider != null)
        {
            Bounds b = currentLadderCollider.bounds;
            float margin = 0.05f; // small padding so we don't sit exactly on the edge

            targetPos.x = Mathf.Clamp(targetPos.x, b.min.x + margin, b.max.x - margin);
            targetPos.z = Mathf.Clamp(targetPos.z, b.min.z + margin, b.max.z - margin);
        }

        // Apply climb + side move in one go
        rb.MovePosition(targetPos);

        // 4) Jump off the ladder (uses the flag we set in Update)
        if (ladderJumpRequested)
        {
            ladderJumpRequested = false;

            isOnLadder = false;
            currentLadder = null;
            currentLadderCollider = null;
            rb.useGravity = true;

            // Stop any leftover ladder velocity before we launch
            rb.linearVelocity = Vector3.zero;

            // Upward + sideways push
            Vector3 jumpDir = Vector3.up * jumpForce;
            Vector3 side = transform.right * -h * ladderSideJumpForce;
            rb.AddForce(jumpDir + side, ForceMode.VelocityChange);

            // NEW: mark that we're flying due to a ladder jump
            _airborneFromLadder = true;

            // Start the cooldown: no rotation allowed while this is > 0
            _postLadderTimer = postLadderNoRotateTime;

            if (anim != null)
            {
                anim.SetBool("IsClimbing", false);
                anim.SetTrigger("Jump");
            }
        }


        // 5) Climb animation while on ladder
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
        // Don't step-up if we're not grounded (prevents popping during airtime)
        if (!IsGrounded())
            return false;


        Vector3 dir = desiredDelta.normalized;
        float dist = desiredDelta.magnitude;

        // 1) try moving from a raised capsule (step up)
        Vector3 up = Vector3.up * stepHeight;
        Vector3 p1Up = p1 + up;
        Vector3 p2Up = p2 + up;

        // must have space at the raised position
        if (Physics.CheckCapsule(p1Up, p2Up, radius, mask, QueryTriggerInteraction.Ignore))
            return false;

        // must be clear to move forward from the raised position
        if (Physics.CapsuleCast(p1Up, p2Up, radius, dir, out _, dist + skin, mask, QueryTriggerInteraction.Ignore))
            return false;

        // 2) apply the raised move
        Vector3 steppedPos = newPos + up + desiredDelta;

        // 3) snap down to ground (so we don't "float")
        Vector3 snapStart = steppedPos + Vector3.up * skin;
        if (Physics.Raycast(snapStart, Vector3.down, out RaycastHit downHit, stepHeight + stepDown, mask, QueryTriggerInteraction.Ignore))
        {
            // only snap onto walkable surfaces
            float slopeCos = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
            if (downHit.normal.y >= slopeCos)
            {
                float targetY = downHit.point.y + skin;

                // Smooth the vertical snap so stairs/mounds don't look like a hop
                steppedPos.y = Mathf.MoveTowards(newPos.y, targetY, stepUpPerFrame);

                newPos = steppedPos;
                return true;

            }
        }

        // If no ground found, still allow the step move (eg small ledge)
        newPos = steppedPos;
        return true;
    }


    // ---------------------- GROUND CHECK ----------------------
    private bool IsGrounded()
    {
        // Simple raycast down from the player; you can adjust distance if needed
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
