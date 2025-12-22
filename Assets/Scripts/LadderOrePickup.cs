using UnityEngine;

public class LadderOrePickup : MonoBehaviour
{
    [Header("Score value for this ore")]
    public int scoreValue = 10;   //gold=10 emerald=20 diamond=30

    [Header("Pickup SFX")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float pickupSfxVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            // counts towards ore
            GameManager.Instance.OreCollected();

            // adds to overall score
            GameManager.Instance.AddScore(scoreValue);
        }

        // play sound at this position
        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupSfxVolume);

        Destroy(gameObject);
    }
}
