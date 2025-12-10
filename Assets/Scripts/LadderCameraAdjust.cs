using UnityEngine;

public class LadderCameraAdjust : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;  // drag PlayerRoot here
    public Camera cam;               // drag this Camera here (or leave empty to auto-find)

    [Header("Pitch (look up/down)")]
    public float extraLadderPitch = -20f;   // negative = look up, positive = look down
    public float rotationLerpSpeed = 5f;

    [Header("FOV")]
    public float ladderFOV = 80f;          // wider on ladder
    public float fovLerpSpeed = 5f;

    private Quaternion _groundLocalRot;
    private Quaternion _ladderLocalRot;
    private float _groundFOV;

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        // Remember how the camera is currently placed (your “normal” tunnel view)
        _groundLocalRot = transform.localRotation;

        // Ladder rotation = original rotation pitched up/down by extraLadderPitch
        _ladderLocalRot = Quaternion.Euler(extraLadderPitch, 0f, 0f) * _groundLocalRot;

        if (cam != null)
            _groundFOV = cam.fieldOfView;
    }

    void LateUpdate()
    {
        if (player == null || cam == null)
            return;

        bool onLadder = player.IsOnLadder;

        // Choose target rotation and FOV
        Quaternion targetRot = onLadder ? _ladderLocalRot : _groundLocalRot;
        float targetFov = onLadder ? ladderFOV : _groundFOV;

        // Smoothly blend to them
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * rotationLerpSpeed
        );

        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFov,
            Time.deltaTime * fovLerpSpeed
        );
    }
}
