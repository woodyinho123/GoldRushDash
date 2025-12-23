using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 
    public PlayerController playerController;

    [Header("Offsets")]
    
    public Vector3 groundOffset;
    public Vector3 ladderOffset = new Vector3(0f, 5f, -7f); // higher and back

    [Header("Look Up")]
    public float groundLookUp = 0f;
    public float ladderLookUp = 4f;

    [Header("FOV")]
    public float groundFOV = 60f;
    public float ladderFOV = 80f;
    public float fovLerpSpeed = 5f;

    [Header("Smoothing")]
    public float positionLerpSpeed = 5f;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();

        if (target != null)
        {
            // camera position as the ground view
            groundOffset = transform.position - target.position;
        }

        if (_cam != null)
        {
            //  groundfov
            groundFOV = _cam.fieldOfView;
        }

        
        if (playerController == null && target != null)
        {
            playerController = target.GetComponentInChildren<PlayerController>();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        bool onLadder = (playerController != null && playerController.IsOnLadder);

        
        Vector3 offset = onLadder ? ladderOffset : groundOffset;
        float lookUp = onLadder ? ladderLookUp : groundLookUp;

        // space follow
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          Time.deltaTime * positionLerpSpeed);

        // Look at player
        Vector3 lookTarget = target.position + Vector3.up * lookUp;
        transform.LookAt(lookTarget);

        // FOV 
        if (_cam != null)
        {
            float targetFov = onLadder ? ladderFOV : groundFOV;
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov,
                                          Time.deltaTime * fovLerpSpeed);
        }
    }
}
