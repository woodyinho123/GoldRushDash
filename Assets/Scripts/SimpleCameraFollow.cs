using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    // The thing we follow (PlayerRoot)
    public Transform target;
    //need to figure out exactly how to set this in the inspector*
    // Offset from the player (relative in world space)
    public Vector3 offset = new Vector3(0f, 2f, -4f);

    // How quickly the camera follows
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Desired camera position
        Vector3 desiredPosition = target.position + offset;

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Always look at the player (slightly above feet)
        Vector3 lookTarget = target.position + Vector3.up * 1.0f;
        transform.LookAt(lookTarget);
    }
}
