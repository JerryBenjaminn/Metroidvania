using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Melee Slash")]
public class MeleeAttackAbility : Ability
{
    [Header("Hitbox")]
    public Vector2 boxSize = new Vector2(1.2f, 0.8f);
    public Vector2 boxOffset = new Vector2(0.8f, 0f);
    public LayerMask targetMask; // Enemy-layer

    [Header("Timing")]
    public float windup = 0.05f;
    public float active = 0.10f;
    public float recovery = 0.10f;

    [Header("Damage")]
    public float damage = 12f;
    public float knockback = 8f;

    [Header("Feedback")]
    public float selfRecoil = 1.5f;         // pieni tönäisy taaksepäin osumalla
    public float victimHitpause = 0.06f;    // uhrille
    public float selfHitpause = 0.02f;      // pelaajalle
    public float cameraShakeAmp = 0.6f;     // 0 jos et halua
    public string attackTrigger = "Attack"; // tyhjä jos ei animaatiota

    public override bool CanUse(IAbilityUser user) => true;

    public override IEnumerator Execute(IAbilityUser user)
    {
        var comp = (Component)user;
        if (!comp) yield break;

        var animator = comp.GetComponentInChildren<Animator>();
        var camShake = Object.FindFirstObjectByType<CameraShake2D>(); // yksinkertainen haku
        var selfPause = comp.GetComponent<HitPause2D>();
        var rb = comp.GetComponent<Rigidbody2D>();
        var sr = comp.GetComponentInChildren<SpriteRenderer>();

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        if (windup > 0) yield return new WaitForSeconds(windup);

        float facing = +1f;
        if (sr) facing = sr.flipX ? -1f : +1f;
        else facing = Mathf.Sign(comp.transform.localScale.x);

        var hitOnce = new HashSet<GameObject>();
        float end = Time.time + Mathf.Max(0.01f, active);

        bool anyHit = false;

        while (Time.time < end)
        {
            Vector2 center = (Vector2)comp.transform.position + new Vector2(boxOffset.x * facing, boxOffset.y);

#if UNITY_EDITOR
            Debug.DrawLine(center + new Vector2(-boxSize.x / 2, -boxSize.y / 2), center + new Vector2(boxSize.x / 2, -boxSize.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(boxSize.x / 2, -boxSize.y / 2), center + new Vector2(boxSize.x / 2, boxSize.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(boxSize.x / 2, boxSize.y / 2), center + new Vector2(-boxSize.x / 2, boxSize.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(-boxSize.x / 2, boxSize.y / 2), center + new Vector2(-boxSize.x / 2, -boxSize.y / 2), Color.red, 0.02f);
#endif

            var hits = Physics2D.OverlapBoxAll(center, boxSize, 0f, targetMask);
            foreach (var h in hits)
            {
                if (!h || hitOnce.Contains(h.gameObject)) continue;
                if (h.transform.root == comp.transform.root) continue;

                anyHit = true;
                hitOnce.Add(h.gameObject);

                // damage + knockback
                if (h.TryGetComponent<IDamageable>(out var dmg))
                {
                    Vector2 dir = ((Vector2)h.transform.position - (Vector2)comp.transform.position).normalized;
                    if (Mathf.Abs(dir.x) < 0.2f) dir.x = facing;
                    dmg.ApplyDamage(damage, dir.normalized * knockback);
                }

                // uhrin hitpause jos löytyy
                if (h.TryGetComponent<HitPause2D>(out var victimPause))
                    victimPause.Pause(victimHitpause);

                // kevyt kameranykäisy
                if (camShake && cameraShakeAmp > 0f)
                    camShake.Shake(cameraShakeAmp, 1.5f);

                // oma recoil ja lyhyt hitpause
                if (rb && selfRecoil > 0f)
                    rb.AddForce(new Vector2(-facing * selfRecoil, 0f), ForceMode2D.Impulse);

                if (selfPause && selfHitpause > 0f)
                    selfPause.Pause(selfHitpause);
            }

            yield return new WaitForFixedUpdate();
        }

        if (recovery > 0) yield return new WaitForSeconds(recovery);
    }
}
