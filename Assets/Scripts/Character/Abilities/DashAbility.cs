using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    public float dashSpeed = 18f;
    public float dashTime = 0.15f;

    public override bool CanUse(IAbilityUser user) => true;

    public override IEnumerator Execute(IAbilityUser user)
    {
        var rb = user.Rigidbody;
        var motor = user.Motor;

        Vector2 dir = new Vector2(Mathf.Sign(user.transform.localScale.x), 0);
        float t = 0f;
        // Halutessa voit disabloida normaalin ohjauksen hetkeksi
        while (t < dashTime) {
            rb.linearVelocity = new Vector2(dir.x * dashSpeed, 0);
            t += Time.deltaTime;
            yield return null;
        }
        yield return Cooldown(cooldown);
    }
}

