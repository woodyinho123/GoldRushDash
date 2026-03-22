using UnityEngine;

public class CheckpointRespawnController : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool checkpointsEnabled = false;
    [SerializeField] private float respawnHealth = 100f;
    [SerializeField] private float deathRespawnDamage = 15f;
    [SerializeField] private float lavaRespawnDamage = 25f;

    private Vector3 checkpointPos;
    private Quaternion checkpointRot;
    private bool hasCheckpoint = false;

    public bool CheckpointsEnabled => checkpointsEnabled;
    public bool CanRespawn => checkpointsEnabled && hasCheckpoint;
    public float RespawnHealth => respawnHealth;
    public float DeathRespawnDamage => deathRespawnDamage;
    public float LavaRespawnDamage => lavaRespawnDamage;

    public void InitializeFromPlayer(Transform playerTransform, float defaultRespawnHealth)
    {
        respawnHealth = defaultRespawnHealth;

        if (playerTransform == null)
        {
            hasCheckpoint = false;
            return;
        }

        checkpointPos = playerTransform.position;
        checkpointRot = playerTransform.rotation;
        hasCheckpoint = true;
    }

    public void SetCheckpoint(Transform checkpointTransform)
    {
        if (!checkpointsEnabled || checkpointTransform == null)
            return;

        checkpointPos = checkpointTransform.position;
        checkpointRot = checkpointTransform.rotation;
        hasCheckpoint = true;
    }

    public bool TryTeleportPlayerToCheckpoint()
    {
        if (!CanRespawn)
            return false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return false;

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = checkpointPos;
            rb.rotation = checkpointRot;
        }
        else
        {
            playerObj.transform.SetPositionAndRotation(checkpointPos, checkpointRot);
        }

        return true;
    }
}