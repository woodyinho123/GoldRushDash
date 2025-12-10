using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // drag PlayerRoot here
    public PlayerController playerController; // drag PlayerRoot here as well

    [Header("Offsets")]
    // Ground offset will be captured from your current camera placement
    public Vector3 groundOffset;
    public Vector3 ladderOffset = new Vector3(0f, 5f, -7f); // higher & back for ladder

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
            // Capture your current scene camera position as the ground view
            groundOffset = transform.position - target.position;
        }

        if (_cam != null)
        {
            // Use whatever FOV you already had as groundFOV
            groundFOV = _cam.fieldOfView;
        }

        // Auto-wire PlayerController if missing
        if (playerController == null && target != null)
        {
            playerController = target.GetComponentInChildren<PlayerController>();
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        bool onLadder = (playerController != null && playerController.IsOnLadder);

        // Choose offset/look-up
        Vector3 offset = onLadder ? ladderOffset : groundOffset;
        float lookUp = onLadder ? ladderLookUp : groundLookUp;

        // Position – world-space follow
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos,
                                          Time.deltaTime * positionLerpSpeed);

        // Look at player (higher when on ladder)
        Vector3 lookTarget = target.position + Vector3.up * lookUp;
        transform.LookAt(lookTarget);

        // FOV blend
        if (_cam != null)
        {
            float targetFov = onLadder ? ladderFOV : groundFOV;
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov,
                                          Time.deltaTime * fovLerpSpeed);
        }
    }
}
