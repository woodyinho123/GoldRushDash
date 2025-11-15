using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput; // Store the movement input

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }

    void Update()
    {
        // Calculate movement direction and apply speed
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement); // Apply movement to Rigidbody
    }

    // This method is called when input is received
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>(); // Get the movement input (WASD or Arrow keys)
    }
}
