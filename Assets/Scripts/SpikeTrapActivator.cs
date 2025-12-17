using UnityEngine;

public class SpikeTrapActivator : MonoBehaviour
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
        if (!other.CompareTag("Player")) return;
        _trap?.Activate();
    }
}