using UnityEngine;

public class RockImpactLevel4 : MonoBehaviour
{
    [Header("Impact Sound")]
    [SerializeField] private AudioClip impactClip;
    [Range(0f, 1f)]
    [SerializeField] private float impactVolume = 0.35f;

    
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    
    private void OnRockImpact()
    {
        if (impactClip == null) return;

       
        GameObject temp = new GameObject("Temp_RockImpactSFX");
        temp.transform.position = transform.position;

        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = impactClip;
        a.volume = Mathf.Clamp01(impactVolume);
        a.spatialBlend = 1f;   
        a.rolloffMode = AudioRolloffMode.Logarithmic;

        if (randomizePitch)
            a.pitch = Random.Range(pitchRange.x, pitchRange.y);

        a.Play();
        Destroy(temp, impactClip.length + 0.1f);
    }
}
