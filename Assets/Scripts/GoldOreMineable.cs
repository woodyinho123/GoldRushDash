using UnityEngine;

public class GoldOreMineable : MonoBehaviour
{
    [Header("Mining Settings")]
    public float miningTime = 2f;      // seconds required to mine
    public float mineEnergyCost = 10f; // energy spent when ore is fully mined

    private float currentMiningTime = 0f;
    private bool isDepleted = false;

    // Called every frame the player is actively mining this ore
    public void Mine(float deltaTime)
    {
        if (isDepleted) return;

        currentMiningTime += deltaTime;

        if (currentMiningTime >= miningTime)
        {
            CompleteMining();
        }
    }

    // Called when the player stops mining or walks away
    public void ResetMining()
    {
        if (isDepleted) return;
        currentMiningTime = 0f;
    }

    private void CompleteMining()
    {
        if (isDepleted) return;
        isDepleted = true;

        // Spend energy once when mining completes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendEnergy(mineEnergyCost);
            GameManager.Instance.OreCollected();
        }

        // Destroy this ore object
        Destroy(gameObject);
    }
}
