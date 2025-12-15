using UnityEngine;
using System.Collections;

public class LadderZone : MonoBehaviour
{
    public float ladderSpeedMultiplierOverride = 1f; // set per ladder zone if desired

    [SerializeField] private float exitGraceTime = 0.12f; // prevents enter/exit flicker

    private Coroutine _pendingExit;
    private PlayerController _currentPc;

    private void OnTriggerEnter(Collider other)
    {
        // Use GetComponentInParent so it works even if the Player tag/collider is on a child
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            _currentPc = pc;

            // Cancel pending exit to avoid flicker
            if (_pendingExit != null)
            {
                StopCoroutine(_pendingExit);
                _pendingExit = null;
            }

            Debug.Log("Entered LADDER trigger: " + gameObject.name);
            pc.SetOnLadder(true, transform);
            pc.ladderSpeedMultiplier = ladderSpeedMultiplierOverride;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponentInParent<PlayerController>();
        if (pc != null && pc == _currentPc)
        {
            if (_pendingExit != null)
                StopCoroutine(_pendingExit);

            _pendingExit = StartCoroutine(ExitAfterDelay(pc));
        }
    }

    private IEnumerator ExitAfterDelay(PlayerController pc)
    {
        yield return new WaitForSeconds(exitGraceTime);

        Debug.Log("Exited LADDER trigger: " + gameObject.name);
        pc.SetOnLadder(false, transform);
        pc.ladderSpeedMultiplier = 1f; // reset so it doesn't affect other ladders

        _pendingExit = null;
        _currentPc = null;
    }
}
