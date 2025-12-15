using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private string checkpointMessage = "CHECKPOINT REACHED!";

    private bool _used = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_used) return;
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(transform, checkpointMessage);
            _used = true;
        }
    }
}
