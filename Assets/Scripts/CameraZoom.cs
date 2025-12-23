using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    public Transform pivot; // drag 

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;          // how fast the zoom changes
    public float minZoomDistance = 0.8f;  // closest allowed
    public float maxZoomDistance = 6f;    // farthest allowed

    [Header("Zoom Height (keeps feet visible)")]
    public float nearHeight = -0.25f;   // camera local Y 
    public float farHeight = 0.15f;    // camera local Y 


    [Header("Over-Shoulder + Look Down (Zoom Assist)")]
    public bool enableOverShoulder = true;

    [Tooltip("Press to switch between right/left shoulder.")]
    public KeyCode switchShoulderKey = KeyCode.C;

    [Tooltip("Horizontal camera offset when zoomed IN (shoulder view).")]
    public float nearShoulderOffset = 0.55f;

    [Tooltip("Horizontal camera offset when zoomed OUT (still slightly shoulder).")]
    public float farShoulderOffset = 0.20f;

    [Tooltip("Tilt DOWN when zoomed IN (degrees).")]
    public float nearPitchDown = 14f;

    [Tooltip("Tilt DOWN when zoomed OUT (degrees).")]
    public float farPitchDown = 6f;

    private int _shoulderSign = 1;     // +1 = right shoulder, -1 = left shoulder
    private float _currentLocalX = 0f; // smoothed local x
    private float _currentPitch = 0f;  // smoothed pitch 
    private Quaternion _baseLocalRot;

    [Header("Smoothing")]
    public float smooth = 12f;

    // internal
    private float _targetDistance;
    private float _currentDistance;
    private Vector3 _baseLocalPos;
    void Awake()
    {
        if (pivot == null)
        {
            Debug.LogWarning("CameraZoom: Pivot not assigned. Disabling.");
            enabled = false;
            return;
        }

        // distance is how far camera is behind pivot 
       
               _currentDistance = Mathf.Clamp(Mathf.Abs(transform.localPosition.z), minZoomDistance, maxZoomDistance);
               _baseLocalPos = transform.localPosition; // - ADD THIS
                _targetDistance = _currentDistance;



        // start from whatever the camera is currently set to
        _currentLocalX = transform.localPosition.x;

        // normalize pitch 
        float startPitch = transform.localEulerAngles.x;
        if (startPitch > 180f) startPitch -= 360f;
        _currentPitch = startPitch;

        _baseLocalRot = transform.localRotation;


    }

    void Update()
    {
        // mouse wheel
        float scroll = Mathf.Clamp(Input.mouseScrollDelta.y, -1f, 1f);

        if (enableOverShoulder && Input.GetKeyDown(switchShoulderKey))
        {
            _shoulderSign *= -1; // swap shoulders
        }


        if (Mathf.Abs(scroll) > 0.01f)
        {
            _targetDistance -= scroll * zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, minZoomDistance, maxZoomDistance);
        }
    }

    void LateUpdate()
    {
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * smooth);

        // 0 = zoomed in 1 = zoomed out
        float t = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _currentDistance);

        Vector3 localPos = _baseLocalPos;

        // distance 
        localPos.z = -_currentDistance;

        // height shifts lower when zoomed in
        localPos.y = Mathf.Lerp(nearHeight, farHeight, t);

        // over-the-shoulder 
        float targetX = enableOverShoulder
            ? _shoulderSign * Mathf.Lerp(nearShoulderOffset, farShoulderOffset, t)
            : 0f;

        _currentLocalX = Mathf.Lerp(_currentLocalX, targetX, Time.deltaTime * smooth);
        localPos.x = _currentLocalX;

        transform.localPosition = localPos;

        // tilt down a bit more when zoomed in 
        float targetPitch = enableOverShoulder
            ? Mathf.Lerp(nearPitchDown, farPitchDown, t)
            : 0f;

        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, Time.deltaTime * smooth);
        transform.localRotation = _baseLocalRot * Quaternion.Euler(_currentPitch, 0f, 0f);


    }

}
