using UnityEngine;

public class SpikeTrapDamageZone : MonoBehaviour
{
    private SpikeTrapAnimatedClip _trap;

    private void Awake()
    {
        _trap = GetComponentInParent<SpikeTrapAnimatedClip>();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        _trap?.OnDamageZoneTouch(other);
    }

    private void OnTriggerStay(Collider other)
    {
        _trap?.OnDamageZoneTouch(other);
    }
}
