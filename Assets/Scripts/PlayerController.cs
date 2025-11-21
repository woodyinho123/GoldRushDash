using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float turnSpeed = 180f;   // in the degrees per second
    public int oreCount = 0;
    public Animator anim;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (anim == null)
        {
            //having problems with player idle and walking anims*
            // This will find the Animator on the Miner child
            anim = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
       // *set initial facing direction
        // If he starts facing the wrong way, change forward to back or tweak* 
       //transform.forward = Vector3.forward; - need tofix this player is facing backwards***
       //removed this altogether, player direction is now correct
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // Left/Right arrows
        float v = Input.GetAxisRaw("Vertical");   // Up/Down arrows 

        // MOVEMENT (forward/back relative to facing) - initital movement wasnt relative to camera, fixed**
        Vector3 move = transform.forward * -v * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        //ROTATION (turn left/right)
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * turnSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // ANIMATION SPEED 
        if (anim != null)
        {
            float speedParam = Mathf.Abs(v); // walk anim when moving forward OR backward, fixed*
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
