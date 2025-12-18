using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ElevatorExitTrigger : MonoBehaviour
{
    private bool used = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
 
    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;


        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver) return;
        if (GameManager.Instance.TimeRemaining <= 0f) return;

        used = true;
        GameManager.Instance.ElevatorExitToNextScene();
    }
}
