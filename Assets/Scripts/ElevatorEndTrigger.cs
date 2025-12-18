using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ElevatorEndTrigger : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip elevatorClip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private bool played = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (played) return;
        if (!other.CompareTag("Player")) return;

        // Only play if we haven't already collapsed
        if (GameManager.Instance != null && GameManager.InstanceIsCollapsedOrGameOver())
            return;

        if (audioSource != null && elevatorClip != null)
            audioSource.PlayOneShot(elevatorClip, volume);

        played = true;
    }
}
