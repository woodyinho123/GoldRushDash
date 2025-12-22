using System.Collections;
using UnityEngine;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Hurt SFX")]
    public AudioSource hurtSource;      
    public AudioClip hurtClip;
    [Range(0f, 1f)] public float hurtVolume = 1f;

    [Header("Flash Red")]
    public Renderer[] flashRenderers;   // assign miner mesh 
    public float flashDuration = 0.15f;
    public Color flashColor = Color.red;

    [Header("Anti-spam")]
    public float minTimeBetweenHurt = 0.08f;

    private float _nextAllowedTime = 0f;
    private MaterialPropertyBlock _mpb;
    private int _baseColorId;
    private int _colorId;
    private Color[] _origColors;
    private Coroutine _co;

    private void Awake()
    {
        if (hurtSource == null)
            hurtSource = GetComponent<AudioSource>();

        if (flashRenderers == null || flashRenderers.Length == 0)
            flashRenderers = GetComponentsInChildren<Renderer>();

        _mpb = new MaterialPropertyBlock();
        _baseColorId = Shader.PropertyToID("_BaseColor"); // lit
        _colorId = Shader.PropertyToID("_Color");         // standard

        _origColors = new Color[flashRenderers.Length];
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null || r.sharedMaterial == null)
            {
                _origColors[i] = Color.white;
                continue;
            }

            if (r.sharedMaterial.HasProperty(_baseColorId))
                _origColors[i] = r.sharedMaterial.GetColor(_baseColorId);
            else if (r.sharedMaterial.HasProperty(_colorId))
                _origColors[i] = r.sharedMaterial.GetColor(_colorId);
            else
                _origColors[i] = Color.white;
        }
    }

    public void PlayHurtFeedback()
    {
        if (Time.time < _nextAllowedTime) return;
        _nextAllowedTime = Time.time + minTimeBetweenHurt;

        // sfx
        if (hurtSource != null && hurtClip != null)
            hurtSource.PlayOneShot(hurtClip, hurtVolume);

        // flash
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FlashCo());
    }

    private IEnumerator FlashCo()
    {
        // set red
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_baseColorId, flashColor);
            _mpb.SetColor(_colorId, flashColor);
            r.SetPropertyBlock(_mpb);
        }

        yield return new WaitForSeconds(flashDuration);

        // restore
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            var r = flashRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_baseColorId, _origColors[i]);
            _mpb.SetColor(_colorId, _origColors[i]);
            r.SetPropertyBlock(_mpb);
        }

        _co = null;
    }
}
