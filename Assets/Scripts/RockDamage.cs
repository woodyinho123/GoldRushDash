using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RockDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 25f;         // how much HEALTH to remove
    public bool instantDeath = false;  // true for lethal rocks

    [Header("Only damage the player once per fall")]
    public bool singleHit = true;

    [Header("Hit Feedback")]
    public AudioClip hitPlayerClip;
    [Range(0f, 1f)] public float hitPlayerVolume = 0.9f;


    private bool _hasHitPlayer;

    private void Reset()
    {
        // Make sure collider is a trigger
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnEnable()
    {
        // reset between spawns
        _hasHitPlayer = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (singleHit && _hasHitPlayer)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
            return;

        if (instantDeath)
        {
            // Take enough damage to guarantee death
            GameManager.Instance.TakeDamage(GameManager.Instance.maxHealth);

            // NEW: hit feedback (flash + sound)
            var pc = other.GetComponentInParent<PlayerController>();
            if (pc != null)
                pc.FlashDamage();

            if (hitPlayerClip != null)
                AudioSource.PlayClipAtPoint(hitPlayerClip, transform.position, hitPlayerVolume);

        }
        else
        {
            GameManager.Instance.TakeDamage(damage);
        }

        _hasHitPlayer = true;
    }
}
