using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;   // faster
    public float turnSpeed = 180f; // degrees per second
    public float moveEnergyPerSecond = 5f;
    public int oreCount = 0;
    public Animator anim;

    [SerializeField] private GameObject miningPromptUI;

    private Rigidbody rb;
    private GoldOreMineable currentOre;    // ore we are standing next to
    private bool isMining = false;         // are we currently mining?
    private bool isRunning = false;        // are we currently running?

    [Header("Footstep Audio")]
    public AudioSource footstepSource;     // looped walking sound

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

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // Left/Right arrows
        float v = Input.GetAxisRaw("Vertical");   // Up/Down arrows 

        // MOVEMENT (disabled while mining)
        if (!isMining)
        {
            // decide if we want to run (hold Left Shift while moving)
            bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(v) > 0.1f;
            isRunning = wantsToRun;

            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            Vector3 move = transform.forward * -v * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

           
            // Only drain energy when SPRINTING (Left Shift held while moving)
            if (isRunning && Mathf.Abs(v) > 0.01f && GameManager.Instance != null)
            {
                GameManager.Instance.SpendEnergy(moveEnergyPerSecond * Time.fixedDeltaTime);
            }


            // ROTATION (turn left/right)
            if (Mathf.Abs(h) > 0.01f)
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

        // ANIMATION SPEED (walking) + running flag
        if (anim != null)
        {
            float speedParam = (!isMining) ? Mathf.Abs(Input.GetAxisRaw("Vertical")) : 0f;
            anim.SetFloat("Speed", speedParam);
            anim.SetBool("IsRunning", isRunning);   // drive running state
        }
    }

    void Update()
    {
        // ---------------- MINING ----------------
        // Are we in range of ore AND holding E?
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

        // Update mining flag
        isMining = wantsToMine;

        // DEBUG see when we think we are mining
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
            anim.SetBool("IsMining", isMining);   // IMPORTANT name must match Animator
        }

        // ----- FOOTSTEPS (looped source on/off) -----
        float moveInput = Input.GetAxisRaw("Vertical");
        bool isWalking = !isMining && Mathf.Abs(moveInput) > 0.1f;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        GoldOreMineable ore = other.GetComponent<GoldOreMineable>();
        if (ore != null)
        {
            currentOre = ore;
            Debug.Log("PlayerController: entered ore trigger");

            if (miningPromptUI != null)        //display mining prompt when near gold ore
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
                miningPromptUI.SetActive(false);  //hide when we leave ore
            }
        }
    }
}
