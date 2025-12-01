using UnityEngine;

public class RockImpactFX : MonoBehaviour
{
    public ParticleSystem impactVFX;
    public AudioSource audioSource;
    public AudioClip impactClip;
    [Range(0f, 1f)] public float impactVolume = 0.9f;

    
    public void OnRockImpact()
    {
        if (impactVFX != null)
        {
            impactVFX.transform.parent = null;
            impactVFX.Play();
            Destroy(impactVFX.gameObject, 3f);
        }

        if (audioSource != null && impactClip != null)
        {
            audioSource.PlayOneShot(impactClip, impactVolume);
        }
    }
}
