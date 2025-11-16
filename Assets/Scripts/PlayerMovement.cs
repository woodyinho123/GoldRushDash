using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Movement speed of the player
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component on the player
    }

    void Update()
    {
        // Get player input for movement
        float moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
        float moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down arrows

        // Calculate movement vector
        Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;

        // Apply movement to Rigidbody to move the player
        rb.MovePosition(transform.position + movement);
    }
}
