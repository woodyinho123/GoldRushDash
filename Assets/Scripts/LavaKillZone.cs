using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LavaKillZone : MonoBehaviour
{
    private void Reset()
    {
        // Ensure this collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnToCheckpoint("YOU FELL IN LAVA!");
        }

    }
}
