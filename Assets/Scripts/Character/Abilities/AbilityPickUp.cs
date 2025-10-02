using UnityEngine;

public class AbilityPickup : MonoBehaviour, IInteractable
{
    [Header("What to unlock")]
    public Ability ability;                  // esim. se DashAbility assetti

    [Header("Persistence (optional)")]
    public string saveId;                    // uniikki, esim. "dash_shrine_01"

    [Header("Feedback (optional)")]
    public Animator animator;                // jos haluat pienen v‰l‰hdyksen
    public string trigger = "Pickup";
    public AudioSource sfx;
    public AudioClip clip;

    public InteractionKind Kind => InteractionKind.Tap;
    public float HoldTime => 0f;

    void Awake()
    {
        if (!string.IsNullOrEmpty(saveId) && PlayerPrefs.GetInt($"pickup_{saveId}", 0) == 1)
            gameObject.SetActive(false); // jo poimittu aiemmin
    }

    public bool CanInteract(GameObject who)
    {
        if (!ability) return false;
        var ac = who.GetComponent<AbilityController>();
        return ac && !ac.IsUnlocked(ability);
    }

    public string GetPrompt()
    {
        return ability ? $"Unlock {ability.name}" : "Unlock ability";
    }

    public void Interact(GameObject who)
    {
        var ac = who.GetComponent<AbilityController>();
        if (!ac || !ability) return;

        if (ac.Unlock(ability))
        {
            if (sfx && clip) sfx.PlayOneShot(clip);
            if (animator && !string.IsNullOrEmpty(trigger)) animator.SetTrigger(trigger);

           /* if (!string.IsNullOrEmpty(saveId))
            {
                PlayerPrefs.SetInt($"pickup_{saveId}", 1);
                PlayerPrefs.Save();
            }
           */

            // voit Destroytaa, tai piilottaa jos haluat audion loppuun
            //Destroy(gameObject);
            gameObject.SetActive(false);
            SaveManager.Instance?.SaveNow();
        }
        else
        {
            // jo unlocked ei mit‰‰n, tai pieni ìalready learnedî -bling
        }
    }

    public void OnFocusEnter(GameObject who) { }
    public void OnFocusExit(GameObject who) { }
}
