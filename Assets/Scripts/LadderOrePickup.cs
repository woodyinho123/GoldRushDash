using UnityEngine;

public class LadderOrePickup : MonoBehaviour
{
    public int scoreValue = 10; // default for gold

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Count this ore in the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OreCollected();
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
