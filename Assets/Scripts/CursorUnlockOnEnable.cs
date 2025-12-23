using UnityEngine;

public class CursorUnlockOnEnable : MonoBehaviour
{
    [SerializeField] private bool showCursor = true;

    private void OnEnable()
    {
        Apply();
    }

    private void Start()
    {
        Apply();
    }

    private void Apply()
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
