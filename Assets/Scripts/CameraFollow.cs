using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform followPoint;
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (followPoint == null) return;

        // Smoothly move camera towards the follow point
        transform.position = Vector3.Lerp(
            transform.position,
            followPoint.position,
            followSpeed * Time.deltaTime
        );

        // Look at the player 
        Transform player = followPoint.parent;
        if (player != null)
        {
            transform.LookAt(player.position + Vector3.up * 1.0f);
        }
    }
}
