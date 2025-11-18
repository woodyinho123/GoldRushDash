
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    public int oreCount = 0;  // how many ore pieces collected
    public Animator anim;   // reference to the miner animator

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    void FixedUpdate()
    { //Needto fix new input system to old***
        // Old Input System axes
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Combine into movement vector
        Vector3 inputDir = new Vector3(h, 0, v);

        // How strong the input is (0 = idle, 1 = full tilt)
        float inputMagnitude = inputDir.magnitude;

        // Tell animator
        if (anim != null)
        {
            anim.SetFloat("Speed", inputMagnitude);
        }

        // Prevent diagonal super speed
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        // Move the rigidbody
        Vector3 move = inputDir * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotate player to face movement direction, need to change backwards to forwards*
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 0.15f));
        }
    }
            private void OnTriggerEnter(Collider other)
    {
        // Check if we touched a Gold Ore trigger
        if (other.CompareTag("GoldOre"))
        {
            oreCount++;
            Debug.Log("Collected ore! Total: " + oreCount);

            // Remove the ore from the scene
            Destroy(other.gameObject);
        }
    


    }
}
