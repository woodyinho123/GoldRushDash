using UnityEngine;

public class LadderOrePickup : MonoBehaviour
{
    [Header("Score value for this ore")]
    public int scoreValue = 10;   // set per prefab (e.g. gold=10, emerald=20, diamond=30)

    [Header("Pickup SFX")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float pickupSfxVolume = 1f;

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

        // Play SFX at this position (safe even though we destroy the ore right after)
        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);

        Destroy(gameObject);
    }
}
