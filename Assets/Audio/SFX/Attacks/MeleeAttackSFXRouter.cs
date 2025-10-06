// MeleeAttackSfxRouter.cs
using UnityEngine;

public class MeleeAttackSfxRouter : MonoBehaviour
{
    public SoundEvent currentSwing; // asetetaan abilitystä joka kerta
    public void SfxSwing()
    {
        if (currentSwing) AudioManager.Instance.Play(currentSwing, transform.position);
        currentSwing = null; // ettei tuplata
    }
}
