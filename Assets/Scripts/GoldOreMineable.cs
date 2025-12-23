using UnityEngine;
using UnityEngine.UI;

public class GoldOreMineable : MonoBehaviour
{
   //mine settings
    public float miningTime = 2f;      // the seconds required to mine
    public float mineEnergyCost = 10f; // energy spent when ore is fully mined

    [Header("VFX and SFX")]
    public ParticleSystem mineVFX;   // child on the ore*
    public AudioClip mineSfx;
    [Range(0f, 1f)] public float mineSfxVolume = 1f;

    [Header("players UI")]
    public Slider miningProgressSlider;

    private float currentMiningTime = 0f;
    private bool isDepleted = false;

    [Header("Score")]
    public int scoreValue = 15;   // or whatever value mined ore should give


    void Start()
    {
       
        if (miningProgressSlider == null)
        {
            miningProgressSlider = GetComponentInChildren<Slider>(true); 
            if (miningProgressSlider == null)
            {
                Debug.LogWarning($"[ore {name}] no slider found in children");
                return;
            }
        }

        
        var canvasGO = miningProgressSlider.transform.parent.gameObject;
        if (!canvasGO.activeSelf)
            canvasGO.SetActive(true);

        miningProgressSlider.minValue = 0f;
        miningProgressSlider.maxValue = 1f;   // we feed it 0–1
        miningProgressSlider.value = 0f;

        
        miningProgressSlider.gameObject.SetActive(false);
    
}



    //MATHS CONTENT PRESENT HERE
    // called when the player is actively mining this ore
    public void Mine(float deltaTime)
    {
        if (isDepleted) return;

        // show the bar when we start mining
        if (miningProgressSlider != null && !miningProgressSlider.gameObject.activeSelf)
        {
            miningProgressSlider.gameObject.SetActive(true);
        }

        currentMiningTime += deltaTime;

        float t = Mathf.Clamp01(currentMiningTime / miningTime);  // 0 / 1

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


    //  when the player stops mining or walks away
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

    //MATHS CONTENT PRESENT HERE
    private void CompleteMining()
    {
        if (isDepleted) return;
        isDepleted = true;

        // hide progress bar
        if (miningProgressSlider != null)
        {
            miningProgressSlider.gameObject.SetActive(false);
        }

        // play vfx
        if (mineVFX != null)
        {
            // detach so it isnt destroyed with the ore
            mineVFX.transform.parent = null;
            mineVFX.Play();
            Destroy(mineVFX.gameObject, 2f);
        }

        // play sfx
        if (mineSfx != null)
        {
            AudioSource.PlayClipAtPoint(mineSfx, transform.position, mineSfxVolume);
        }

        // spend energy  inform gamemanager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendEnergy(mineEnergyCost);
            GameManager.Instance.OreCollected();

            // ADD SCORE ONCE when mining completes
            GameManager.Instance.AddScore(scoreValue);
        }


        // remove this ore
        Destroy(gameObject);
    }
}
