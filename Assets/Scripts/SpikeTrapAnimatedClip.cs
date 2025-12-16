using System.Collections;
using UnityEngine;

public class SpikeTrapAnimatedClip : MonoBehaviour
{
    [Header("References")]
    public Animator spikesAnimator;         // drag the Animator from corridor_spike_trap
    public Collider damageTrigger;          // drag Trigger BoxCollider here
    public string raiseStateName = "Raise"; // state name in Animator Controller

    [Header("Timing")]
    public float stayExtendedTime = 0.25f;
    public float cooldownTime = 0.75f;

    [Header("Damage")]
    public float damage = 20f;
    public bool damageOncePerActivation = true;

    private bool _running = false;
    private float _cooldown = 0f;
    private bool _damagedThisCycle = false;

    private void Awake()
    {
        if (damageTrigger == null)
            damageTrigger = GetComponentInChildren<Collider>();

        if (spikesAnimator == null)
            spikesAnimator = GetComponentInChildren<Animator>();

        // Start retracted (first frame), paused
        if (spikesAnimator != null)
        {
            spikesAnimator.Play(raiseStateName, 0, 0f);
            spikesAnimator.speed = 0f;
        }
    }

    private void Update()
    {
        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_running)
        {
            TryDamage();
            return;
        }

        if (_cooldown > 0f) return;

        StartCoroutine(TrapRoutine());
        TryDamage();
    }

    private void TryDamage()
    {
        if (damageOncePerActivation && _damagedThisCycle) return;
        _damagedThisCycle = true;

        if (GameManager.Instance != null)
            GameManager.Instance.TakeDamage(damage);
    }

    private IEnumerator TrapRoutine()
    {
        _running = true;
        _damagedThisCycle = false;

        // Extend (forward)
        spikesAnimator.speed = 1f;
        spikesAnimator.Play(raiseStateName, 0, 0f);
        yield return WaitForNormalized(0.98f);

        // Hold
        spikesAnimator.speed = 0f;
        yield return new WaitForSeconds(stayExtendedTime);

        // Retract (reverse)
        spikesAnimator.speed = -1f;
        spikesAnimator.Play(raiseStateName, 0, 1f);
        yield return WaitForNormalized(0.02f);

        // Freeze retracted
        spikesAnimator.speed = 0f;
        spikesAnimator.Play(raiseStateName, 0, 0f);

        _running = false;
        _cooldown = cooldownTime;
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
