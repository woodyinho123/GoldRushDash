using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LavaKillZone : MonoBehaviour
{
    private void Reset()
    {
        // Make sure the collider is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react to the player
        if (!other.CompareTag("Player")) return;

        // Instant death by draining all health
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(GameManager.Instance.maxHealth);
        }
    }
}
