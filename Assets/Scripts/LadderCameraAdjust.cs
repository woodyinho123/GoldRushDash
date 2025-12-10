using UnityEngine;

public class LadderCameraAdjust : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;  // drag PlayerRoot here
    public Camera cam;               // drag this Camera here (or leave empty to auto-find)

    [Header("Pitch (look up/down)")]
    public float extraLadderPitch = -20f;   // negative = look up
    public float rotationLerpSpeed = 5f;

    [Header("FOV")]
    public float ladderFOV = 80f;          // wider on ladder
    public float fovLerpSpeed = 5f;

    [Header("Ladder hang time")]
    public float ladderHoldTime = 0.4f;    // seconds to keep ladder view after leaving ladder

    private Quaternion _groundLocalRot;
    private Quaternion _ladderLocalRot;
    private float _groundFOV;

    private float _ladderTimer = 0f;

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        // Remember the normal camera orientation
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

        bool onLadderNow = player.IsOnLadder;

        // If we're currently on a ladder, refresh the "ladder context" timer
        if (onLadderNow)
        {
            _ladderTimer = ladderHoldTime;
        }
        else if (_ladderTimer > 0f)
        {
            _ladderTimer -= Time.deltaTime;
        }

        // We consider ourselves in ladder context while timer > 0
        bool ladderContext = onLadderNow || _ladderTimer > 0f;

        // Choose target rotation and FOV
        Quaternion targetRot = ladderContext ? _ladderLocalRot : _groundLocalRot;
        float targetFov = ladderContext ? ladderFOV : _groundFOV;

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
