using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    private LichMovement lichMovement;

    void Awake()
    {
        // Find the LichMovement script on the parent
        lichMovement = GetComponentInParent<LichMovement>();
        if (lichMovement == null)
        {
            Debug.LogError("LichMovement script not found on parent!");
        }
    }

    // Method to call the parent's PlayLightningAttackSound
    public void PlayLightningAttackSound()
    {
        if (lichMovement != null)
        {
            lichMovement.PlayLightningAttackSound();
        }
    }
    public void PlaySummonPatrollerSound()
    {
        if (lichMovement != null)
        {
            lichMovement.PlaySummonPatrollerSound();
        }
    }
    public void PlaySummonFlyerSound()
    {
        if (lichMovement != null)
        {
            lichMovement.PlaySummonFlyerSound();
        }
    }
    public void PlayHomingProjectileSound()
    {
        if (lichMovement != null)
        {
            lichMovement.PlayHomingProjectileSound();
        }
    }
    public void PlayLightningStormSound()
    {
        if(lichMovement != null)
        {
            lichMovement.PlayLightningStormSound();
        }
    }
}