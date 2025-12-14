using UnityEngine;

public class LadderZone : MonoBehaviour
{
    public float ladderSpeedMultiplierOverride = 1f; // set per ladder zone if desired

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Debug.Log("Entered LADDER trigger: " + gameObject.name);
            pc.SetOnLadder(true, transform);
            pc.ladderSpeedMultiplier = ladderSpeedMultiplierOverride;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            Debug.Log("Exited LADDER trigger: " + gameObject.name);
            pc.SetOnLadder(false, transform);
            pc.ladderSpeedMultiplier = 1f; // reset so it doesn't affect other ladders

        }
    }

}
