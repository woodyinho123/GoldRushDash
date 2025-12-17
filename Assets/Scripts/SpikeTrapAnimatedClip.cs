using System.Collections;
using UnityEngine;

public class SpikeTrapAnimatedClip : MonoBehaviour
{
    [Header("References")]
    public Animator spikesAnimator;         // Animator on SpikesVisual
    public string raiseStateName = "Raise"; // Animator state name

    [Header("Damage Zone")]
    [Tooltip("The trigger volume that damages the player (DamageZone).")]
    public Collider damageZone;

    [Header("Timing")]
    public float stayExtendedTime = 0.25f;
    public float cooldownTime = 0.75f;

    [Header("Damage")]
    public float damage = 20f;
    public float damageTickCooldown = 0.35f;  // prevents rapid drain if standing on spikes

    [Header("Spike Raise SFX")]
    public AudioSource spikeSfxSource;   // optional; if null we’ll try GetComponent<AudioSource>()
    public AudioClip spikeRaiseClip;
    [Range(0f, 1f)] public float spikeRaiseVolume = 1f;


    private bool _running = false;
    private float _cooldown = 0f;

    private bool _damageArmed = false;
    private float _nextDamageTime = 0f;

    private void Awake()
    {
        if (spikeSfxSource == null)
            spikeSfxSource = GetComponent<AudioSource>();

        if (spikesAnimator == null)
            spikesAnimator = GetComponentInChildren<Animator>();

        // Start retracted + paused
        if (spikesAnimator != null)
        {
            spikesAnimator.Play(raiseStateName, 0, 0f);
            spikesAnimator.speed = 0f;
        }

        // Damage off by default
        SetDamageArmed(false);
    }

    private void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    // Called by ActivationTrigger script
    public void Activate()
    {
        if (_running) return;
        if (_cooldown > 0f) return;

        StartCoroutine(TrapRoutine());
    }

    // Called by DamageZone script
    public void OnDamageZoneTouch(Collider other)
    {
       
        if (!_damageArmed) return;
        if (!other.CompareTag("Player")) return;

        if (Time.time < _nextDamageTime) return;
        _nextDamageTime = Time.time + damageTickCooldown;

        var feedback = other.GetComponentInParent<PlayerDamageFeedback>();
        if (feedback != null)
            feedback.PlayHurtFeedback();

        if (GameManager.Instance != null)
            GameManager.Instance.TakeDamage(damage);
    }

    private IEnumerator TrapRoutine()
    {
        _running = true;

        if (spikeSfxSource != null && spikeRaiseClip != null)
            spikeSfxSource.PlayOneShot(spikeRaiseClip, spikeRaiseVolume);

        // EXTEND
        spikesAnimator.speed = 1f;
        spikesAnimator.Play(raiseStateName, 0, 0f);

        // Arm damage near the top (so player has a brief reaction window)
        yield return WaitForNormalized(0.35f);
        SetDamageArmed(true);

        // Wait until the animation finishes
        yield return WaitForNormalized(0.98f);

        // HOLD at top
        spikesAnimator.speed = 0f;
        yield return new WaitForSeconds(stayExtendedTime);

        // RETRACT
        SetDamageArmed(false);
        spikesAnimator.speed = -1f;
        spikesAnimator.Play(raiseStateName, 0, 1f);

        // Wait until it reaches start
        yield return WaitForNormalized(0.02f);

        // Freeze retracted
        spikesAnimator.speed = 0f;
        spikesAnimator.Play(raiseStateName, 0, 0f);

        _running = false;
        _cooldown = cooldownTime;
    }

    private void SetDamageArmed(bool armed)
    {
        _damageArmed = armed;

        // optional: enable/disable the collider itself (keeps things simple & cheap)
        if (damageZone != null)
            damageZone.enabled = armed;
    }

    private IEnumerator WaitForNormalized(float target)
    {
        while (true)
        {
            var s = spikesAnimator.GetCurrentAnimatorStateInfo(0);
            float t = Mathf.Repeat(s.normalizedTime, 1f);

            if (spikesAnimator.speed > 0f)
            {
                if (t >= target) break;
            }
            else if (spikesAnimator.speed < 0f)
            {
                if (t <= target) break;
            }
            else break;

            yield return null;
        }
    }
}
