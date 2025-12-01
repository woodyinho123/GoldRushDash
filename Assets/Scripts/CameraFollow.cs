using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                // dragging playerroot here
    public Vector3 offset = new Vector3(0f, 2f, -6f); // above and behind player*

    void LateUpdate()
    {
        if (!target) return;
        //change 2d maze controls to behind camera controlls***
        // world space follow instead of maze relative
        transform.position = target.position + offset;

        // always look at the player
        transform.LookAt(target.position);
    }
}
//i need to fix the camera circling and jitter which happens occasionally**