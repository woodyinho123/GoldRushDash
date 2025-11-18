using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    // The thing we follow (PlayerRoot)
    public Transform target;

    // Distance behind the player
    public float distance = 4f;

    // Height above the player
    public float height = 2f;

    // How fast we follow / rotate
    public float followSpeed = 10f;

    // Where on the player to look (0 = feet, 1 = head-ish)
    public float lookHeight = 1.0f;

    void LateUpdate()
    {
        if (target == null) return;

        // Behind the player, based on which way HE is facing
        Vector3 behind = -target.forward; // opposite of where he looks

        // Desired position for the camera
        Vector3 desiredPos = target.position
                             + behind * distance
                             + Vector3.up * height;

        // Smooth position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );

        // Look at the player (a bit above his centre)
        Vector3 lookPos = target.position + Vector3.up * lookHeight;
        Quaternion desiredRot = Quaternion.LookRotation(lookPos - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            followSpeed * Time.deltaTime
        );
    }
}
