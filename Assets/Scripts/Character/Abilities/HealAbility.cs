using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/HealAbility")]
public class HealAbility : Ability
{
    [Header("Healing")]
    public float healAmount = 1f;      // 1 sydän
    public int apCost = 33;            // paljonko AbilityPoweria kuluu
    public float castTime = 0.6f;      // valinnainen “focus”-fiilis
    public bool lockMovement = true;

    public SoundEvent startSfx;        // valinnainen
    public SoundEvent completeSfx;     // valinnainen

    public override bool CanUse(IAbilityUser user)
    {
        var go = (user as MonoBehaviour)?.gameObject;
        if (!go) return false;
        var ap = go.GetComponent<AbilityPower>();
        var hp = go.GetComponent<Health>();
        if (!ap || !hp) return false;

        if (hp.Current >= hp.Max) return false;      // täynnä ei viitsi
        return ap.Current >= apCost;                 // onko varaa
    }

    public override IEnumerator Execute(IAbilityUser user)
    {
        var go = (user as MonoBehaviour)?.gameObject;
        var motor = user.Motor;
        var rb = user.Rigidbody;
        var ap = go.GetComponent<AbilityPower>();
        var hp = go.GetComponent<Health>();
        if (!ap || !hp) yield break;

        if (!ap.TrySpend(apCost)) yield break;

        if (startSfx) AudioManager.Instance.Play(startSfx, go.transform.position);

        var storedVel = rb ? rb.linearVelocity : Vector2.zero;
        if (lockMovement && motor)
        {
            motor.enabled = false;
            if (rb) rb.linearVelocity = Vector2.zero;
        }

        // pieni “focus” viive
        if (castTime > 0f) yield return new WaitForSeconds(castTime);

        hp.Heal(healAmount);
        if (completeSfx) AudioManager.Instance.Play(completeSfx, go.transform.position);

        if (lockMovement && motor)
        {
            motor.enabled = true;
            if (rb) rb.linearVelocity = storedVel;
        }
    }
}
