using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Dash")]
public class DashAbility : Ability
{
    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashTime = 0.15f;

    [Header("Control Locks")]
    public bool lockFacingDuringDash = true;
    public bool lockMoveDuringDash = true;

    [Header("Physics")]
    public bool zeroGravityDuringDash = true;  // kevyempi, tasalaatuinen dash
    public bool preserveYInAir = false; // jos true, s‰ilyt‰ ilmassa nykyinen y-nopeus

    [Header("Input")]
    [Range(0f, 1f)] public float aimDeadzoneX = 0.25f; // tatti/nuoli ohittaa facingin vasta kun ylitt‰‰ t‰m‰n

    public override bool CanUse(IAbilityUser user) => true;

    public override IEnumerator Execute(IAbilityUser user)
    {
        var comp = (Component)user;
        var rb = user.Rigidbody ?? comp.GetComponent<Rigidbody2D>();
        var motor = user.Motor;

        if (!rb) yield break;

        // 1) P‰‰t‰ suunta: AimInput.x > deadzone ? sen suunta : motor.Facing
        var aim = comp.GetComponent<AimInput>()?.Aim ?? Vector2.zero;
        int facing = motor ? motor.Facing
                           : (comp.GetComponentInChildren<SpriteRenderer>()?.flipX == true ? -1 : 1);
        int dirX = Mathf.Abs(aim.x) > aimDeadzoneX ? (aim.x > 0 ? 1 : -1) : facing;

        // 2) Lukitse ohjaus hetkeksi, ettei moottori k‰‰nn‰ p‰‰t‰/kiihdyt‰ takaisin
        if (lockFacingDuringDash) motor?.LockFacing(dashTime);
        if (lockMoveDuringDash) motor?.Lockmove(dashTime);

        // 3) Valinnainen painovoiman nollaus dashin ajaksi
        float prevGrav = rb.gravityScale;
        if (zeroGravityDuringDash) rb.gravityScale = 0f;

        float t = 0f;
        float baseVy = rb.linearVelocity.y;

        while (t < dashTime)
        {
            var v = rb.linearVelocity;
            v.x = dirX * dashSpeed;
            v.y = preserveYInAir ? baseVy : 0f;
            rb.linearVelocity = v;

            t += Time.deltaTime;
            yield return null; // frame-by-frame on ok, koska asetamme suoraan velocityn
        }

        if (zeroGravityDuringDash) rb.gravityScale = prevGrav;

        yield return Cooldown(cooldown);
    }
}
