using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RockDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 25f;         // how much health to remove
    public bool instantDeath = false;  // true for lethal rocks

    [Header("Only damage the player once per fall")]
    public bool singleHit = true;

    [Header("Hurt Feedback (Player)")]
     public bool playHurtFeedback = true;

    [Header("Hit Feedback")]
    public AudioClip hitPlayerClip;
    [Range(0f, 1f)] public float hitPlayerVolume = 0.9f;


    private bool _hasHitPlayer;

    private void Reset()
    {
        //   collider is a trigger?
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnEnable()
    {
        // reset between spawns
        _hasHitPlayer = false;
    }

                 private void OnTriggerEnter(Collider other)
      {
          if (singleHit && _hasHitPlayer)
              return;
  
          if (!other.CompareTag("Player"))
              return;
 
         if (GameManager.Instance == null)
             return;
 
         // hurt feedback
         if (playHurtFeedback)
         {
              var feedback = other.GetComponentInParent<PlayerDamageFeedback>();
              if (feedback != null)
                  feedback.PlayHurtFeedback();
          }

          if (instantDeath)
                 GameManager.Instance.TakeDamage(GameManager.Instance.maxHealth);
         else
                GameManager.Instance.TakeDamage(damage);

          _hasHitPlayer = true;
     }

    }


