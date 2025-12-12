using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("References")]
    public Transform pivot; // drag Focal Point here

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;          // how fast the zoom changes
    public float minZoomDistance = 0.8f;  // closest allowed (prevents clipping into player)
    public float maxZoomDistance = 6f;    // farthest allowed

    [Header("Zoom Height (keeps feet visible)")]
    public float nearHeight = -0.25f;   // camera local Y when zoomed IN
    public float farHeight = 0.15f;    // camera local Y when zoomed OUT

    [Header("Smoothing")]
    public float smooth = 12f;

    // internal
    private float _targetDistance;
    private float _currentDistance;

    void Awake()
    {
        if (pivot == null)
        {
            Debug.LogWarning("CameraZoom: Pivot not assigned. Disabling.");
            enabled = false;
            return;
        }

        // distance is how far camera is behind pivot (local -Z usually)
        Vector3 local = pivot.InverseTransformPoint(transform.position);
        _currentDistance = -local.z;
        _targetDistance = _currentDistance;
    }

    void Update()
    {
        // Mouse wheel: positive usually zooms in/out depending on mouse.
        float scroll = Mathf.Clamp(Input.mouseScrollDelta.y, -1f, 1f);


        if (Mathf.Abs(scroll) > 0.01f)
        {
            _targetDistance -= scroll * zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, minZoomDistance, maxZoomDistance);
        }
    }

    void LateUpdate()
    {
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * smooth);

        // 0 = zoomed in, 1 = zoomed out
        float t = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _currentDistance);

        Vector3 localPos = transform.localPosition;

        // distance (behind pivot)
        localPos.z = -_currentDistance;

        // height shifts lower when zoomed in (so you see feet)
        localPos.y = Mathf.Lerp(nearHeight, farHeight, t);

        transform.localPosition = localPos;
    }

}
