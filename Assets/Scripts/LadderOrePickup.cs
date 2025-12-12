using UnityEngine;

public class LadderOrePickup : MonoBehaviour
{
    [Header("Score value for this ore")]
    public int scoreValue = 10;   // set per prefab (e.g. gold=10, emerald=20, diamond=30)

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            // counts towards "Ore: X / Y"
            GameManager.Instance.OreCollected();

            // adds to overall score
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}
