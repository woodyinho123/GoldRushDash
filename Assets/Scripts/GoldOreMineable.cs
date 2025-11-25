using UnityEngine;

public class GoldOreMineable : MonoBehaviour
{
    [Header("Mining Settings")]
    public float miningTime = 2f;      // seconds required to mine
    public float mineEnergyCost = 10f; // energy spent when ore is fully mined

    [Header("VFX / SFX")]
    public ParticleSystem mineVFX;   // child on the ore
    public AudioClip mineSfx;
    [Range(0f, 1f)] public float mineSfxVolume = 1f;

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

        // Play VFX
        if (mineVFX != null)
        {
            // Detach so it isn't destroyed with the ore
            mineVFX.transform.parent = null;
            mineVFX.Play();
            Destroy(mineVFX.gameObject, 2f);
        }

        // Play SFX
        if (mineSfx != null)
        {
            AudioSource.PlayClipAtPoint(mineSfx, transform.position, mineSfxVolume);
        }

        // Spend energy & inform GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendEnergy(mineEnergyCost);
            GameManager.Instance.OreCollected();
        }

        // Remove this ore
        Destroy(gameObject);
    }
}
