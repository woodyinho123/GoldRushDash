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

    [Header("Position (local)")]
    public float ladderHeightOffset = -0.25f; // negative = camera goes LOWER on ladder
    public float positionLerpSpeed = 5f;


    [Header("Ladder hang time")]
    public float ladderHoldTime = 0.4f;    // seconds to keep ladder view after leaving ladder

    [Header("Ground check (for ladder-to-ladder jumps)")]
    public float groundCheckDistance = 1.2f;     // increase if your player is tall
    public LayerMask groundMask = ~0;            // set to your ground layers if needed


    private Quaternion _groundLocalRot;
    private Quaternion _ladderLocalRot;
    private float _groundFOV;
    private Vector3 _groundLocalPos;
    private Vector3 _ladderLocalPos;


    private float _ladderTimer = 0f;

    void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        // Remember the normal camera orientation
        _groundLocalRot = transform.localRotation;

        // Remember the normal camera local position
        _groundLocalPos = transform.localPosition;

        // Ladder position = ground position + a vertical offset
        _ladderLocalPos = _groundLocalPos + new Vector3(0f, ladderHeightOffset, 0f);


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

        // Simple grounded check (so ladder camera doesn't drop mid-air between ladders)
        Vector3 origin = player.transform.position + Vector3.up * 0.1f;
        bool groundedNow = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (onLadderNow)
        {
            // While on ladder, keep refreshing the hold timer
            _ladderTimer = ladderHoldTime;
        }
        else
        {
            // If we're NOT grounded (airborne), do NOT count the timer down.
            // This keeps ladder camera active for ladder-to-ladder jumps.
            if (groundedNow && _ladderTimer > 0f)
                _ladderTimer -= Time.deltaTime;
        }

        // Ladder context = on ladder, OR still holding, OR airborne (between ladders)
        bool ladderContext = onLadderNow || _ladderTimer > 0f;



        // Choose target rotation, position and FOV
        Quaternion targetRot = ladderContext ? _ladderLocalRot : _groundLocalRot;
        Vector3 targetPos = ladderContext ? _ladderLocalPos : _groundLocalPos;
        float targetFov = ladderContext ? ladderFOV : _groundFOV;


        // Smoothly blend to them
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * rotationLerpSpeed
        );

        transform.localPosition = Vector3.Lerp(
    transform.localPosition,
    targetPos,
    Time.deltaTime * positionLerpSpeed
);


        cam.fieldOfView = Mathf.Lerp(
            cam.fieldOfView,
            targetFov,
            Time.deltaTime * fovLerpSpeed
        );
    }
}
