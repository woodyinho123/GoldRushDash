using System.Collections;
using UnityEngine;

public class SpikeTrapAnimated : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator that plays the spike raise animation (on SpikesVisual).")]
    public Animator spikesAnimator;

    [Tooltip("Exact state name in the Animator (e.g. Raise).")]
    public string raiseStateName = "Raise";

    [Header("Timing")]
    [Tooltip("How long to stay fully extended before retracting.")]
    public float stayExtendedTime = 0.25f;

    [Tooltip("Cooldown before the trap can trigger again.")]
    public float cooldownTime = 0.75f;

    [Header("Damage")]
    public float damage = 20f;
    public bool damageOncePerActivation = true;

    private bool _running = false;
    private float _cooldown = 0f;
    private bool _damagedThisCycle = false;

    private void Awake()
    {
        if (spikesAnimator == null)
            spikesAnimator = GetComponentInChildren<Animator>();

        //  set animation to first frame and pause it*
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

        // damage shouldnt move player- we only call TakeDamage
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
    //MATHS CONTENT PRESENT HERE
    private IEnumerator TrapRoutine()
    {
        _running = true;
        _damagedThisCycle = false;

        // EXTEND
        spikesAnimator.speed = 1f;
        spikesAnimator.Play(raiseStateName, 0, 0f);

        //  until the animation reaches the end
        yield return WaitForStateNormalizedTime(0.98f);

        // hold 
        spikesAnimator.speed = 0f;
        yield return new WaitForSeconds(stayExtendedTime);

        // play reverse
        spikesAnimator.speed = -1f;
        spikesAnimator.Play(raiseStateName, 0, 1f);

        // wait until it reaches the start
        yield return WaitForStateNormalizedTime(0.02f);

        // freeze retracted
        spikesAnimator.speed = 0f;
        spikesAnimator.Play(raiseStateName, 0, 0f);

        _running = false;
        _cooldown = cooldownTime;
    }
    //MATHS CONTENT PRESENT HERE
    private IEnumerator WaitForStateNormalizedTime(float target)
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
            else
            {
                break;
            }

            yield return null;
        }
    }
}
