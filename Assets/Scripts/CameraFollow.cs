using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                       // drag PlayerRoot here
    public Vector3 offset = new Vector3(0f, 3f, -8f); // tweak to taste

    void LateUpdate()
    {
        if (target == null) return;

        // Simple world-space offset behind + above the player
        transform.position = target.position + offset;

        // Look slightly above the player’s center
        Vector3 lookPos = target.position + Vector3.up * 1.2f;
        transform.LookAt(lookPos);
    }
}
