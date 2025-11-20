using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 180f;   // degrees per second
    public int oreCount = 0;
    public Animator anim;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (anim == null)
        {
            // This will find the Animator on the Miner child
            anim = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        // Optional: set initial facing direction.
        // If he starts facing the wrong way, change forward to back or tweak in the editor.
       //transform.forward = Vector3.forward;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // Left/Right arrows or A/D
        float v = Input.GetAxisRaw("Vertical");   // Up/Down arrows or W/S

        // ----- MOVEMENT (forward/back relative to facing) -----
        Vector3 move = transform.forward * -v * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // ----- ROTATION (turn left/right) -----
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * turnSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // ----- ANIMATION SPEED -----
        if (anim != null)
        {
            float speedParam = Mathf.Abs(v); // walk anim when moving forward OR backward
            anim.SetFloat("Speed", speedParam);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoldOre"))
        {
            oreCount++;
            Debug.Log("Collected ore! Total: " + oreCount);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OreCollected();
            }

            Destroy(other.gameObject);
        }
    }
}
