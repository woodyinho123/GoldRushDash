using UnityEngine;

public class ShoulderZoomCamera : MonoBehaviour
{
    [Header("Zoom Step (one notch)")]
    public float zoomForwardLocal = 0.6f;     // moves camera forward (towards player
    public float shoulderOffsetLocal = 0.35f; // moves camera sideways
    public float tiltDownDegrees = 6f;        // adds pitch down after zoom
    


    [Header("Smoothing")]
    public float moveLerpSpeed = 12f;
    public float rotLerpSpeed = 12f;

    [Header("Controls")]
    public KeyCode switchShoulderKey = KeyCode.C;

    private Vector3 _baseLocalPos;
    private Quaternion _baseLocalRot;

    private bool _zoomed = false;
    private bool _rightShoulder = true;

    private void Start()
    {
        //  store exactly where the camera starts and always return to it
        _baseLocalPos = transform.localPosition;
        _baseLocalRot = transform.localRotation;
    }

    private void Update()
    {
        // mouse wheel: one step zoom in/out
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0.05f)
            _zoomed = true;

        if (scroll < -0.05f)
            _zoomed = false;

        // c key: swap shoulder side
        if (Input.GetKeyDown(switchShoulderKey))
            _rightShoulder = !_rightShoulder;
    }
    //MATHS CONTENT PRESENT HERE
    private void LateUpdate()
    {
        
        Vector3 targetPos = _baseLocalPos;

        if (_zoomed)
        {
            float sideSign = _rightShoulder ? 1f : -1f;

            
            targetPos += new Vector3(sideSign * shoulderOffsetLocal, 0f, zoomForwardLocal);
        }

        //local rotation
        Quaternion targetRot = _baseLocalRot;

        if (_zoomed)
        {
            // tilt down slightly
            targetRot = _baseLocalRot * Quaternion.Euler(tiltDownDegrees, 0f, 0f);
        }

        // add smooth motion
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * moveLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * rotLerpSpeed);
    }
}
