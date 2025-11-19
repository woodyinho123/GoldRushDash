using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                // drag PlayerRoot here
    public Vector3 offset = new Vector3(0f, 2f, -6f); // above + behind player

    void LateUpdate()
    {
        if (!target) return;

        // World-space follow
        transform.position = target.position + offset;

        // Always look at the player
        transform.LookAt(target.position);
    }
}
