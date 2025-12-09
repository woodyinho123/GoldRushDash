using UnityEngine;

public class LadderOrePickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Count this ore in the GameManager, same as mined ones
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OreCollected();
        }

        // Destroy ore object after pickup
        Destroy(gameObject);
    }
}
