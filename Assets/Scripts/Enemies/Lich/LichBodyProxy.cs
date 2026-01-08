using UnityEngine;

public class LichBodyProxy : MonoBehaviour
{
    public LichMovement lichMovement;

    void Awake()
    {
        if (lichMovement == null)
        {
            lichMovement = GetComponentInParent<LichMovement>();
        }

        if (lichMovement == null)
        {
            Debug.LogError("LichMovement script not found on parent GameObject!");
        }
    }

    public void PlayLightningStormSound()
    {
        if (lichMovement != null)
        {
            lichMovement.PlayLightningStormSound();
        }
        else
        {
            Debug.LogError("LichMovement reference is missing!");
        }
    }
}