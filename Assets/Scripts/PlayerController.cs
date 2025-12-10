using UnityEngine;
using UnityEngine.InputSystem.XR;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;        // faster
    public float turnSpeed = 180f;     // degrees per second
    public float moveEnergyPerSecond = 5f;
    public int oreCount = 0;
    public Animator anim;

    [SerializeField] private GameObject miningPromptUI;

    private Rigidbody rb;
    private GoldOreMineable currentOre;    // ore we are standing next to
    private bool isMining = false;         // are we currently mining?
    private bool isRunning = false;        // are we currently running?

    [Header("Jump")]
    public float jumpForce = 6f;          // used when jumping OFF ladders

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

    [Header("Ladder Jump")]
    public float postLadderNoRotateTime = 0.3f; // how long after ladder jump we block rotation
    private float _postLadderTimer = 0f;

    [Header("Fall Damage")]
    // Negative value: e.g. -18 means "if we hit the ground while falling faster than -18"
    public float fatalFallSpeed = -18f;
    private bool wasGrounded = false;
    private float lastVerticalSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

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
                bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(v) > 0.1f;
                isRunning = wantsToRun;

                float currentSpeed = isRunning ? runSpeed : walkSpeed;

                // move forward/back along local forward
                Vector3 move = transform.forward * -v * currentSpeed * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + move);

                // Only drain energy when SPRINTING
                if (isRunning && Mathf.Abs(v) > 0.01f && GameManager.Instance != null)
                {
                    GameManager.Instance.SpendEnergy(moveEnergyPerSecond * Time.fixedDeltaTime);
                }

                // ROTATION (turn left/right) - DISABLED during post-ladder cooldown
                bool canRotate = (_postLadderTimer <= 0f);   // <- key condition

                if (canRotate && Mathf.Abs(h) > 0.01f)
                {
                    float turnAmount = h * turnSpeed * Time.fixedDeltaTime;
                    Quaternion deltaRotation = Quaternion.Euler(0f, turnAmount, 0f);
                    rb.MoveRotation(rb.rotation * deltaRotation);
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
            ladderJumpRequested = false;   // consume request

            isOnLadder = false;
            currentLadder = null;
            currentLadderCollider = null;
            rb.useGravity = true;

            // Stop any leftover ladder velocity before we launch
            rb.linearVelocity = Vector3.zero;

            // Upward + sideways push
            Vector3 jumpDir = Vector3.up * jumpForce;
            Vector3 side = transform.right * -h * ladderSideJumpForce; // minus = A=left, D=right
            rb.AddForce(jumpDir + side, ForceMode.VelocityChange);

            // Start the cooldown: no rotation allowed while this is > 0
            _postLadderTimer = postLadderNoRotateTime;

            if (anim != null)
            {
                anim.SetBool("IsClimbing", false);
                anim.SetTrigger("Jump");   // play jump animation
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

    // ---------------------- GROUND CHECK ----------------------
    private bool IsGrounded()
    {
        // Simple raycast down from the player; you can adjust distance if needed
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}
