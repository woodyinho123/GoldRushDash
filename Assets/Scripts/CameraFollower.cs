using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform followPoint;

    void LateUpdate()
    {
        if (followPoint == null) return;

        // Just copy the position & rotation of the follow point
        transform.position = followPoint.position;
        transform.rotation = followPoint.rotation;
    }
}
