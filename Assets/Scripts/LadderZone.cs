using UnityEngine;
using System.Collections;

public class LadderZone : MonoBehaviour
{
    public float ladderSpeedMultiplierOverride = 1f;
    [SerializeField] private float exitGraceTime = 0.12f;

    private Coroutine _pendingExit;
    private PlayerController _currentPc;

    // NEW: how many colliders belonging to the player are inside this trigger
    private int _overlapCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        // If a different player ever enters, ignore (single-player game)
        if (_currentPc != null && pc != _currentPc) return;

        _currentPc = pc;
        _overlapCount++;

        // cancel pending exit so we don't flicker
        if (_pendingExit != null)
        {
            StopCoroutine(_pendingExit);
            _pendingExit = null;
        }

        // Only “enter ladder” on the first collider entering
        if (_overlapCount == 1)
        {
            pc.SetOnLadder(true, transform);
            pc.ladderSpeedMultiplier = ladderSpeedMultiplierOverride;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;
        if (pc != _currentPc) return;

        _overlapCount = Mathf.Max(0, _overlapCount - 1);

        // If any collider is still inside, don't exit ladder
        if (_overlapCount > 0) return;

        if (_pendingExit != null)
            StopCoroutine(_pendingExit);

        _pendingExit = StartCoroutine(ExitAfterDelay(pc));
    }

    private IEnumerator ExitAfterDelay(PlayerController pc)
    {
        yield return new WaitForSeconds(exitGraceTime);

        // If re-entered during grace time, do nothing
        if (_overlapCount > 0)
        {
            _pendingExit = null;
            yield break;
        }

        pc.SetOnLadder(false, transform);
        pc.ladderSpeedMultiplier = 1f;

        _pendingExit = null;
        _currentPc = null;
    }
}
