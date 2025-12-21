using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpikeTrapDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 15f;

    [Header("Optional hurt feedback on player")]
    public bool playHurtFeedback = true;

    private bool _damageWindowOpen = false;
    private bool _hasDamagedThisCycle = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // Called by animation events
    public void EnableDamageWindow()
    {
        _damageWindowOpen = true;
        _hasDamagedThisCycle = false; // new spike cycle starts -> allow 1 hit
    }

    // Called by animation events
    public void DisableDamageWindow()
    {
        _damageWindowOpen = false;
        _hasDamagedThisCycle = false; // reset for next cycle
    }

    private void OnTriggerEnter(Collider other) => TryDamage(other);

    private void OnTriggerStay(Collider other)
    {
        // Important: if player is already standing in it when spikes rise,
        // OnTriggerEnter won't fire, so Stay is needed.
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!_damageWindowOpen) return;
        if (_hasDamagedThisCycle) return;
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsGameOver) return;

        if (playHurtFeedback)
        {
            var feedback = other.GetComponentInParent<PlayerDamageFeedback>();
            if (feedback != null) feedback.PlayHurtFeedback();
        }

        GameManager.Instance.TakeDamage(damage);
        _hasDamagedThisCycle = true;
    }
}
