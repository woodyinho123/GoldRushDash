using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    public int oreCount = 0;
    public Animator anim;       // drag Miner Animator here in Inspector

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        // Force the starting facing direction down the tunnel.
        // Try one, and if he still looks at the camera, swap to the other.

        // Option A:
        transform.forward = Vector3.back;

        // If that makes him look the wrong way, comment that out and use:
        // transform.forward = Vector3.back;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0f, v);

        float inputMagnitude = inputDir.magnitude;
        if (inputMagnitude > 1f) inputDir.Normalize();

        if (anim != null)
        {
            anim.SetFloat("Speed", inputMagnitude);
        }

        if (inputMagnitude > 0.001f)
        {
            Vector3 move = inputDir * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            // Model needs flipped direction, so keep using -inputDir
            Quaternion targetRot = Quaternion.LookRotation(-inputDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 0.15f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoldOre"))
        {
            oreCount++;
            Debug.Log("Collected ore! Total: " + oreCount);
            Destroy(other.gameObject);
        }
    }
}
