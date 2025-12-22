using UnityEngine;
using System.Collections;

public class LadderZone : MonoBehaviour
{
    public float ladderSpeedMultiplierOverride = 1f;
    [SerializeField] private float exitGraceTime = 0.12f;

    private Coroutine _pendingExit;
    private PlayerController _currentPc;

    // how many colliders belonging to the player are inside this trigger
    private int _overlapCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        // if a different player ever enters, ignore
        if (_currentPc != null && pc != _currentPc) return;

        _currentPc = pc;
        _overlapCount++;

        // cancel pending exit so we dont flicker
        if (_pendingExit != null)
        {
            StopCoroutine(_pendingExit);
            _pendingExit = null;
        }

        // only enter ladder on the first collider enter
        if (_overlapCount == 1)
        {
            pc.SetOnLadder(true, transform);
            pc.ladderSpeedMultiplier = ladderSpeedMultiplierOverride;
        }
    }
    //MATHS CONTENT PRESENT HERE
    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;
        if (pc != _currentPc) return;

        _overlapCount = Mathf.Max(0, _overlapCount - 1);

        // if any collider is still inside dont exit ladder
        if (_overlapCount > 0) return;

        if (_pendingExit != null)
            StopCoroutine(_pendingExit);

        _pendingExit = StartCoroutine(ExitAfterDelay(pc));
    }

    private IEnumerator ExitAfterDelay(PlayerController pc)
    {
        yield return new WaitForSeconds(exitGraceTime);

        // if reentered during  time do nothing
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
