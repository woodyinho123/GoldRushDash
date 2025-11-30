using UnityEngine;

public class RockTriggerZone : MonoBehaviour
{
    public RockLinearMotion rockToDrop;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (rockToDrop != null) rockToDrop.Drop();
    }
}
