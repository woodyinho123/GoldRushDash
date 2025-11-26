using UnityEngine;
using UnityEngine.UI;

public class GoldOreMineable : MonoBehaviour
{
    [Header("Mining Settings")]
    public float miningTime = 2f;      // seconds required to mine
    public float mineEnergyCost = 10f; // energy spent when ore is fully mined

    [Header("VFX / SFX")]
    public ParticleSystem mineVFX;   // child on the ore
    public AudioClip mineSfx;
    [Range(0f, 1f)] public float mineSfxVolume = 1f;

    [Header("UI")]
    public Slider miningProgressSlider;

    private float currentMiningTime = 0f;
    private bool isDepleted = false;

    void Start()
    {
        // Auto-find slider if nothing is assigned in the Inspector
        if (miningProgressSlider == null)
        {
            miningProgressSlider = GetComponentInChildren<Slider>(true);  // true = include inactive
            if (miningProgressSlider == null)
            {
                Debug.LogWarning($"[Ore {name}] No Slider found in children!");
                return;
            }
        }

        // Make sure the parent canvas is enabled
        var canvasGO = miningProgressSlider.transform.parent.gameObject;
        if (!canvasGO.activeSelf)
            canvasGO.SetActive(true);

        miningProgressSlider.minValue = 0f;
        miningProgressSlider.maxValue = 1f;   // we feed it 0–1
        miningProgressSlider.value = 0f;

        // Start with just the slider hidden (not the whole canvas)
        miningProgressSlider.gameObject.SetActive(false);
    
}




    // Called every frame the player is actively mining this ore
    public void Mine(float deltaTime)
    {
        if (isDepleted) return;

        // Show the bar when we start mining
        if (miningProgressSlider != null && !miningProgressSlider.gameObject.activeSelf)
        {
            miningProgressSlider.gameObject.SetActive(true);
        }

        currentMiningTime += deltaTime;

        float t = Mathf.Clamp01(currentMiningTime / miningTime);  // 0 → 1

        if (miningProgressSlider != null)
        {
            miningProgressSlider.value = t;

            Debug.Log($"[Ore {name}] Mining t={t:0.00}, sliderActive={miningProgressSlider.gameObject.activeInHierarchy}");
        }

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

        if (miningProgressSlider != null)
        {
            miningProgressSlider.value = 0f;
            miningProgressSlider.gameObject.SetActive(false);
        }
    }


    private void CompleteMining()
    {
        if (isDepleted) return;
        isDepleted = true;

        // Hide the progress bar
        if (miningProgressSlider != null)
        {
            miningProgressSlider.gameObject.SetActive(false);
        }

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
