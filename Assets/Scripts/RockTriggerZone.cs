using UnityEngine;
using System.Collections;

public class RockTriggerZone : MonoBehaviour
{
    [Header("Rock to control")]
    public FallingRock rockToDrop;

    [Header("Timing")]
    [Tooltip("Delay between player entering the trigger and rock starting to fall.")]
    public float dropDelay = 0.8f;

    [Header("Warning FX")]
    [Tooltip("Optional: AudioSource with a warning clip (rumble, crack, etc).")]
    public AudioSource warningAudio;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        Debug.Log($"RockTriggerZone: Player entered, will drop rock {rockToDrop} after {dropDelay} sec");

        // Play warning sound immediately when player enters
        if (warningAudio != null)
        {
            warningAudio.Play();
        }

        if (rockToDrop != null)
        {
            StartCoroutine(DelayedDrop());
        }
        else
        {
            Debug.LogWarning($"RockTriggerZone: No rockToDrop assigned on {name}");
        }
    }

    private IEnumerator DelayedDrop()
    {
        // Wait before starting the fall
        yield return new WaitForSeconds(dropDelay);

        if (rockToDrop != null)
        {
            rockToDrop.Drop();
        }
    }
}
