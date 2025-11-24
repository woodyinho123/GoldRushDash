using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 180f;         // degrees per second
    public float moveEnergyPerSecond = 5f;
    public int oreCount = 0;
    public Animator anim;

    private Rigidbody rb;
    private GoldOreMineable currentOre;    // ore we are standing next to
    private bool isMining = false;         // are we currently mining?

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
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // Left/Right arrows
        float v = Input.GetAxisRaw("Vertical");   // Up/Down arrows 

        // ----- MOVEMENT (disabled while mining) -----
        if (!isMining)
        {
            Vector3 move = transform.forward * -v * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            // Only drain energy when actually moving
            if (Mathf.Abs(v) > 0.01f && GameManager.Instance != null)
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

        // ----- ANIMATION SPEED (walking) -----
        if (anim != null)
        {
            float speedParam = (!isMining) ? Mathf.Abs(Input.GetAxisRaw("Vertical")) : 0f;
            anim.SetFloat("Speed", speedParam);
        }
    }

    void Update()
    {
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

        // DEBUG: see when we think we are mining
        if (anim != null && isMining)
        {
            Debug.Log("Setting IsMining TRUE on Animator: " + anim.gameObject.name);
        }

        // Drive mining animation
        if (anim != null)
        {
            anim.SetBool("IsMining", isMining);   // IMPORTANT: name must match Animator
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GoldOreMineable ore = other.GetComponent<GoldOreMineable>();
        if (ore != null)
        {
            currentOre = ore;
            Debug.Log("PlayerController: entered ore trigger");
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
        }
    }
}
