using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Melee Slash")]
public class MeleeAttackAbility : Ability
{
    [Header("Hitbox")]
    public Vector2 boxSize = new Vector2(1.2f, 0.8f);
    public Vector2 boxOffset = new Vector2(0.8f, 0f);        // et�isyys eteenp�in
    public LayerMask targetMask;                              // Enemy-layer

    [Header("Timing")]
    public float windup = 0.06f;   // ennen osumaa
    public float active = 0.10f;   // ikkuna jolloin osuu
    public float recovery = 0.12f; // pienen hetken �loppuviive�

    [Header("Damage")]
    public float damage = 12f;
    public float knockback = 8f;

    [Header("Animation (optional)")]
    public string attackTrigger = "Attack"; // j�t� tyhj�ksi jos et k�yt�

    public override bool CanUse(IAbilityUser user) => true;

    public override IEnumerator Execute(IAbilityUser user)
    {
        // yritet��n trigata animaatio jos l�ytyy
        var comp = user as Component;
        var animator = comp ? comp.GetComponentInChildren<Animator>() : null;
        if (animator && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        // halutessa voisi lukita liikkeen tms. nyt menn��n simppelill�
        if (windup > 0) yield return new WaitForSeconds(windup);

        // osumaikkuna: skannaa alueen useamman kerran ja ly� jokainen kohde max kerran
        var hitOnce = new HashSet<GameObject>();
        float end = Time.time + Mathf.Max(0.01f, active);

        // yritet��n p��tell� facing: SpriteRenderer.flipX tai localScale.x
        float facing = +1f;
        var sr = comp ? comp.GetComponentInChildren<SpriteRenderer>() : null;
        if (sr) facing = sr.flipX ? -1f : +1f;
        else if (comp) facing = Mathf.Sign(comp.transform.localScale.x);

        while (Time.time < end)
        {
            if (!comp) yield break;
            Vector2 center = (Vector2)comp.transform.position + new Vector2(boxOffset.x * facing, boxOffset.y);

#if         UNITY_EDITOR
            // PIIRR� DEBUG-LAATIKKO T�SS�, JOKA FRAME
            DrawDebugBox(center, boxSize, facing);
#endif
            var hits = Physics2D.OverlapBoxAll(center, boxSize, 0f, targetMask);

            foreach (var h in hits)
            {
                if (!h || hitOnce.Contains(h.gameObject)) continue;
                // �l� ly� itse�
                if (comp && h.transform.root == comp.transform.root) continue;

                if (h.TryGetComponent<IDamageable>(out var dmg))
                {
                    // suunta: v�h�n kohdetta kohti, painotus eteenp�in
                    Vector2 dir = ((Vector2)h.transform.position - (Vector2)comp.transform.position).normalized;
                    if (Mathf.Abs(dir.x) < 0.2f) dir.x = facing; // varmistus ett� tulee selke� t�n�isy
                    dmg.ApplyDamage(damage, dir.normalized * knockback);
                    hitOnce.Add(h.gameObject);
                }
            }

            // sampletaa fyysisill� frameilla
            yield return new WaitForFixedUpdate();
        }

        if (recovery > 0) yield return new WaitForSeconds(recovery);
    }

    
}
