using UnityEngine;

public class RockImpactLevel4Smoke : MonoBehaviour
{
    [Header("Smoke (spawns on impact)")]
    [SerializeField] private ParticleSystem smokePoofPrefab;
    [SerializeField] private float extraLifetime = 0.5f;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    private bool _spawned;


    private void OnRockImpact()
    {
        if (_spawned) return;
        _spawned = true;

        if (smokePoofPrefab == null) return;

        ParticleSystem ps = Instantiate(
            smokePoofPrefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );

       
        ps.Play(true);

        float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        Destroy(ps.gameObject, lifetime + extraLifetime);
    }
}
