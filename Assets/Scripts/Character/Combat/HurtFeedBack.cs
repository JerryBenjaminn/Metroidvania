using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Health))]
public class HurtFeedback : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] SpriteRenderer[] renderers;      // jos tyhjä, haetaan automaattisesti lapsista
    [SerializeField] Color flashColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] float flashOn = 0.06f;
    [SerializeField] float flashOff = 0.06f;
    [SerializeField] bool useBlink = true;           // true = blinkkaa väriä; false = pelkkä Hurt-trigger

    [Header("Animation (optional)")]
    [SerializeField] Animator animator;
    [SerializeField] string hurtTrigger = "Hurt";

    Health health;
    Coroutine flashCo;
    List<Color> originalColors = new List<Color>();

    void Awake()
    {
        health = GetComponent<Health>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        originalColors.Clear();
        foreach (var r in renderers) originalColors.Add(r ? r.color : Color.white);
    }

    void OnEnable() { health.OnDamaged += OnDamaged; }
    void OnDisable() { health.OnDamaged -= OnDamaged; RestoreColors(); }

    void OnDamaged(float amount, Vector2 dir)
    {
        if (animator && !string.IsNullOrEmpty(hurtTrigger))
            animator.SetTrigger(hurtTrigger);

        if (!useBlink) return;

        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(FlashFor(health.InvulnSeconds));
    }

    IEnumerator FlashFor(float duration)
    {
        float end = Time.time + duration;
        bool state = false;

        while (Time.time < end)
        {
            state = !state;
            ApplyFlash(state);
            yield return new WaitForSeconds(state ? flashOn : flashOff);
        }

        ApplyFlash(false);
        flashCo = null;
    }

    void ApplyFlash(bool on)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;
            r.color = on ? flashColor : originalColors[i];
        }
    }

    void RestoreColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r) r.color = originalColors[i];
        }
    }
}
