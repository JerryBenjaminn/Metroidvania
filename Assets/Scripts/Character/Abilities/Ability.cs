using System.Collections;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public float cooldown = 0.2f;

    public virtual bool CanUse(IAbilityUser user) => true;
    public abstract IEnumerator Execute(IAbilityUser user);

    // helperi cooldownille
    protected IEnumerator Cooldown(float seconds) {
        yield return new WaitForSeconds(seconds);
    }
}

