using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SlashDir { Forward, Up, Down }

[CreateAssetMenu(menuName = "Abilities/Melee Slash (Directional)")]
public class MeleeAttackAbility : Ability
{
    [Header("Hitbox: Forward")]
    public Vector2 fwdBoxSize = new Vector2(1.2f, 0.8f);
    public Vector2 fwdBoxOffset = new Vector2(0.8f, 0f);

    [Header("Hitbox: Up")]
    public Vector2 upBoxSize = new Vector2(0.9f, 1.1f);
    public Vector2 upBoxOffset = new Vector2(0.2f, 0.9f);

    [Header("Hitbox: Down")]
    public Vector2 downBoxSize = new Vector2(0.9f, 1.1f);
    public Vector2 downBoxOffset = new Vector2(0.2f, -0.9f);

    [Header("Targets")]
    public LayerMask targetMask; // Enemy-layer

    [Header("Timing")]
    public float windup = 0.05f;
    public float active = 0.10f;
    public float recovery = 0.10f;

    [Header("Damage & Knockback")]
    public float damage = 12f;
    public float knockbackFwd = 8f;
    public float knockbackUp = 8f;
    public float knockbackDown = 8f;

    [Header("Pogo (Down Slash)")]
    public bool enablePogo = true;
    public float pogoUpVelocity = 12f;     // aseta isommaksi kuin tavallinen hyppy, esim 12ñ14
    public float pogoMinAirSpeedY = -1f;   // jos olit nousemassa, leikkaa y nopeus ennen pogoa

    [Header("Feedback")]
    public float selfRecoilFwd = 1.5f;
    public float selfRecoilUp = 0.6f;
    public float selfRecoilDown = 0.0f;
    public float victimHitpause = 0.06f;
    public float selfHitpause = 0.02f;
    public float cameraShakeAmp = 0.6f;

    [Header("Animation")]
    public string trigForward = "Attack";
    public string trigUp = "AttackUp";
    public string trigDown = "AttackDown";

    [Header("Control Lock")]
    public bool lockFacingDuringAttack = true;
    public bool includeRecoveryInLock = true;

    [Header("SFX")]
    public SoundEvent swingWhiffSfx;
    public SoundEvent hitEnemySfx;
    public bool playWhiffAtActivate = true;

    public int apOnHit = 5;
    public int apOnKill = 15;

    public override bool CanUse(IAbilityUser user) => true;

    public override IEnumerator Execute(IAbilityUser user)
    {
        var comp = (Component)user; if (!comp) yield break;
        var motor = user.Motor;
        var rb = user.Rigidbody;
        if (!rb) rb = comp.GetComponent<Rigidbody2D>();     // fallback, ettei j‰‰ nulliksi
        if (!rb) Debug.LogWarning("[MeleeAttack] Rigidbody2D puuttuu k‰ytt‰j‰lt‰, pogo/recoil eiv‰t toimi.");
        var animator = comp.GetComponentInChildren<Animator>();
        var sr = comp.GetComponentInChildren<SpriteRenderer>();
        var aimCmp = comp.GetComponent<AimInput>();
        var camShake = Object.FindFirstObjectByType<CameraShake2D>();
        var selfPause = comp.GetComponent<HitPause2D>();

        // p‰‰t‰ suunta
        Vector2 aim = aimCmp ? aimCmp.Aim : Vector2.zero;
        SlashDir dir = SlashDir.Forward;
        if (aim.y > 0.5f) dir = SlashDir.Up;
        else if (aim.y < -0.5f) dir = SlashDir.Down;

        // lukitse facing iskuajaksi
        if (lockFacingDuringAttack && motor != null)
        {
            float lockTime = windup + active + (includeRecoveryInLock ? recovery : 0f);
            motor.LockFacing(lockTime);
        }

        // animaatio
        if (animator)
        {
            string trig = dir == SlashDir.Up ? trigUp : dir == SlashDir.Down ? trigDown : trigForward;
            if (!string.IsNullOrEmpty(trig)) animator.SetTrigger(trig);
        }

        var router = comp.GetComponent<MeleeAttackSfxRouter>();
        if (router) router.currentSwing = swingWhiffSfx;

        if (windup > 0) yield return new WaitForSeconds(windup);

        float facing = sr && sr.flipX ? -1f : (motor != null ? motor.Facing : Mathf.Sign(comp.transform.localScale.x));

        // aktiivivaihe
        var hitOnce = new HashSet<GameObject>();
        float end = Time.time + Mathf.Max(0.01f, active);
        bool anyHit = false;
        bool hitPlayed = false;

        while (Time.time < end)
        {

            Vector2 center; Vector2 size;
            switch (dir)
            {
                case SlashDir.Up:
                    size = upBoxSize;
                    center = (Vector2)comp.transform.position + new Vector2(upBoxOffset.x * facing, upBoxOffset.y);
                    break;
                case SlashDir.Down:
                    size = downBoxSize;
                    center = (Vector2)comp.transform.position + new Vector2(downBoxOffset.x * facing, downBoxOffset.y);
                    break;
                default:
                    size = fwdBoxSize;
                    center = (Vector2)comp.transform.position + new Vector2(fwdBoxOffset.x * facing, fwdBoxOffset.y);
                    break;
            }

#if UNITY_EDITOR
            Debug.DrawLine(center + new Vector2(-size.x / 2, -size.y / 2), center + new Vector2(size.x / 2, -size.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(size.x / 2, -size.y / 2), center + new Vector2(size.x / 2, size.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(size.x / 2, size.y / 2), center + new Vector2(-size.x / 2, size.y / 2), Color.red, 0.02f);
            Debug.DrawLine(center + new Vector2(-size.x / 2, size.y / 2), center + new Vector2(-size.x / 2, -size.y / 2), Color.red, 0.02f);
#endif
            
            var hits = Physics2D.OverlapBoxAll(center, size, 0f, targetMask);
            foreach (var h in hits)
            {
                if (!h || hitOnce.Contains(h.gameObject)) continue;
                if (h.transform.root == comp.transform.root) continue;

                anyHit = true;
                hitOnce.Add(h.gameObject);
               
                // knockback-suunta
                Vector2 kbDir;
                float kbPow;

                switch (dir)
                {
                    case SlashDir.Up:
                        kbDir = Vector2.up + new Vector2(facing * 0.2f, 0f);
                        kbPow = knockbackUp;
                        break;
                    case SlashDir.Down:
                        kbDir = Vector2.down + new Vector2(facing * 0.1f, 0f);
                        kbPow = knockbackDown;
                        break;
                    default:
                        kbDir = new Vector2(facing, 0f);
                        kbPow = knockbackFwd;
                        break;
                }

                if (hitEnemySfx && !hitPlayed)
                {
                    AudioManager.Instance.Play(hitEnemySfx, comp.transform.position);
                    hitPlayed = true;
                }

                var attackerGo = (user as MonoBehaviour)?.gameObject;
                if (!attackerGo) yield break;

                var attackerAP = attackerGo.GetComponent<AbilityPower>();
                 

                if (attackerAP != null)
                {
                    attackerAP.Gain(apOnHit);                   
                }

                if (h.TryGetComponent<IDamageable>(out var dmg))
                    dmg.ApplyDamage(damage, kbDir.normalized * kbPow);

                if (h.TryGetComponent<HitPause2D>(out var victimPause) && victimHitpause > 0f)
                    victimPause.Pause(victimHitpause);

                if (camShake && cameraShakeAmp > 0f)
                    camShake.Shake(cameraShakeAmp, 1.5f);
            }

            yield return new WaitForFixedUpdate();
        }

        // omat palautteet osumasta
        if (anyHit && rb)
        {
            if (selfPause && selfHitpause > 0f) selfPause.Pause(selfHitpause);

            if (rb)
            {
                switch (dir)
                {
                    case SlashDir.Up:
                        if (selfRecoilUp > 0f) rb.AddForce(new Vector2(-facing * 0.5f, -selfRecoilUp), ForceMode2D.Impulse);
                        break;
                    case SlashDir.Down:
                        if (enablePogo && !user.IsGrounded)
                        {
                            var v = rb.linearVelocity;
                            if (v.y > pogoMinAirSpeedY) v.y = pogoMinAirSpeedY; // leikkaa ylˆsp‰in ennen hyppy‰
                            v.y = pogoUpVelocity;
                            rb.linearVelocity = v;
                        }
                        if (selfRecoilDown > 0f) rb.AddForce(new Vector2(-facing * selfRecoilDown, 0f), ForceMode2D.Impulse);
                        break;
                    default:
                        if (selfRecoilFwd > 0f) rb.AddForce(new Vector2(-facing * selfRecoilFwd, 0f), ForceMode2D.Impulse);
                        break;
                }
            }
        }

        if (recovery > 0) yield return new WaitForSeconds(recovery);
    }
}
