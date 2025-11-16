using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Move the player based on the input
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);
    }

    // Called when player provides movement input
    public void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>(); // Get the movement input from the new Input System
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + movement);  // Apply movement to Rigidbody
    }
}
