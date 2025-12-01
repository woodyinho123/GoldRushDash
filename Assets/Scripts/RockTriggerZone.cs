using UnityEngine;

public class RockTriggerZone : MonoBehaviour
{
    [Tooltip("The FallingRock this trigger will drop.")]
    public FallingRock rockToDrop;

    [Tooltip("Trigger only once, even if the player re-enters.")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // only react to player
        if (!other.CompareTag("Player"))
            return;

        // if we already triggered and we only want it once then ignore
        if (triggerOnce && hasTriggered)
            return;

        if (rockToDrop != null)
        {
            Debug.Log("RockTriggerZone: Player entered, dropping rock " + rockToDrop.name);
            rockToDrop.Drop();
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning("RockTriggerZone: No rockToDrop assigned on " + name);
        }
    }
}
