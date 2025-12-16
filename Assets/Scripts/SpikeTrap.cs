using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spikeVisual;     // the part that moves up/down
    [SerializeField] private Collider damageTrigger;    // MUST be trigger (no pushing)

    [Header("Movement")]
    [SerializeField] private float extendHeight = 0.9f; // local Y amount to rise
    [SerializeField] private float extendTime = 0.08f;  // fast up
    [SerializeField] private float holdTime = 0.25f;    // stay up briefly
    [SerializeField] private float retractTime = 0.12f; // down

    [Header("Damage")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private bool damageOncePerActivation = true;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1.0f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip spikeSfx;
    [Range(0f, 1f)][SerializeField] private float spikeSfxVolume = 1f;

    private Vector3 _retractedLocalPos;
    private Vector3 _extendedLocalPos;

    private bool _busy = false;
    private bool _didDamageThisActivation = false;
    private float _nextAllowedTime = 0f;

    private void Reset()
    {
        // Best-effort auto-wire if you forget
        spikeVisual = transform.Find("SpikeVisual");
        var trig = transform.Find("DamageTrigger");
        if (trig != null) damageTrigger = trig.GetComponent<Collider>();

        if (damageTrigger != null)
            damageTrigger.isTrigger = true;
    }

    private void Awake()
    {
        if (spikeVisual == null)
            Debug.LogError("SpikeTrap: spikeVisual not assigned.");

        if (damageTrigger == null)
            Debug.LogError("SpikeTrap: damageTrigger not assigned.");

        if (damageTrigger != null)
            damageTrigger.isTrigger = true;

        if (spikeVisual != null)
        {
            _retractedLocalPos = spikeVisual.localPosition;
            _extendedLocalPos = _retractedLocalPos + Vector3.up * extendHeight;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // We want the trigger to live on the SAME object as this script,
        // OR you can put this script on the parent and the trigger on the child.
        // Either way: make sure the collider that receives this event is the trigger.
        if (!other.CompareTag("Player")) return;

        if (Time.time < _nextAllowedTime) return;
        if (_busy) return;

        StartCoroutine(TrapRoutine());
    }

    private IEnumerator TrapRoutine()
    {
        _busy = true;
        _didDamageThisActivation = false;
        _nextAllowedTime = Time.time + cooldown;

        // Extend quickly
        yield return MoveSpike(spikeVisual.localPosition, _extendedLocalPos, extendTime);

        // Optional SFX
        if (spikeSfx != null)
            AudioSource.PlayClipAtPoint(spikeSfx, transform.position, spikeSfxVolume);

        // Hold up briefly
        yield return new WaitForSeconds(holdTime);

        // Retract
        yield return MoveSpike(spikeVisual.localPosition, _retractedLocalPos, retractTime);

        _busy = false;
    }

    private IEnumerator MoveSpike(Vector3 from, Vector3 to, float duration)
    {
        if (spikeVisual == null) yield break;

        if (duration <= 0.0001f)
        {
            spikeVisual.localPosition = to;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            spikeVisual.localPosition = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        spikeVisual.localPosition = to;
    }

    // Put this on the same GameObject as the trigger collider,
    // OR forward the trigger events from your DamageTrigger child.
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_busy) return;

        // Only damage while spikes are "up-ish"
        // (simple check: if we've moved above halfway)
        if (spikeVisual == null) return;

        float progress = Mathf.InverseLerp(_retractedLocalPos.y, _extendedLocalPos.y, spikeVisual.localPosition.y);
        if (progress < 0.5f) return;

        if (damageOncePerActivation && _didDamageThisActivation) return;

        if (GameManager.Instance != null)
        {
            // Matches your existing hazard style (same as RockDamage) :contentReference[oaicite:1]{index=1}
            GameManager.Instance.TakeDamage(damage);
        }

        _didDamageThisActivation = true;
    }
}
