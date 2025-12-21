using UnityEngine;

public class SpikeTrapDamageZone : MonoBehaviour
{
    private SpikeTrapAnimatedClip _trap;
    [SerializeField] private float hitCooldown = 0.75f;
    private float _nextAllowedHitTime = 0f;

    private void Awake()
    {

        _trap = GetComponentInParent<SpikeTrapAnimatedClip>();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
    private void TryDamage(Collider other)
    {
        if (Time.time < _nextAllowedHitTime) return;

        _trap?.OnDamageZoneTouch(other);
        _nextAllowedHitTime = Time.time + hitCooldown;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

}
