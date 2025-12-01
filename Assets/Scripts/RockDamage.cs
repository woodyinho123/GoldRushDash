using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RockDamage : MonoBehaviour
{
    [Header("Damage")]
    public float energyDamage = 25f;
    public bool instantDeath = false;   // true for lethal rocks
    [Header("Only damage the player once per fall")]
    public bool singleHit = true;

    private bool _hasHitPlayer;

    private void Reset()
    {
        // make collider a trigger here
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHitPlayer && singleHit) return;

        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                if (instantDeath)
                {
                    
                    var gm = GameManager.Instance;
                    var method = gm.GetType().GetMethod("LoseGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null) method.Invoke(gm, new object[] { "Crushed by falling rocks!" });
                }
                else
                {
                    GameManager.Instance.SpendEnergy(energyDamage);
                }
            }

            _hasHitPlayer = true;
        }
    }
}
