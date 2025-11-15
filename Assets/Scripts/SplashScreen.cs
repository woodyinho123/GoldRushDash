using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Movement speed for the player
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();  // Get the Rigidbody component attached to the player
    }

    void Update()
    {
        // Get input from player (WASD or Arrow keys)
        float moveX = Input.GetAxis("Horizontal");  // A/D or Left/Right arrows
        float moveZ = Input.GetAxis("Vertical");    // W/S or Up/Down arrows
        //still need to add speed for player*
        // Calculate movement direction and apply speed
        Vector3 movement = new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;

        // Applying movement to player’s Rigidbody
        rb.MovePosition(transform.position + movement);
    }
}
