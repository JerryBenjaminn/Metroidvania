using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] float max = 100;
    public float Max => max;
    public float Current { get; private set; }

    [Header("Damage Response")]
    [SerializeField] bool applyKnockback = true;
    [SerializeField] float knockbackScale = 1f;
    [SerializeField] bool useInvulnerability = true;
    [SerializeField] float invulnSeconds = 0.15f;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;          // current, max
    public event Action<float, Vector2> OnDamaged;             // amount, hitDir  <-- UUSI

    public float InvulnSeconds => invulnSeconds;               // <-- UUSI
    public bool IsInvulnerable => invuln;                      // <-- UUSI

    Rigidbody2D rb;
    bool invuln;

    void Awake()
    {
        Current = max;
        rb = GetComponent<Rigidbody2D>();
    }
    // Health.cs
    public void SetHealthFromSave(float value)
    {
        Current = Mathf.Clamp(value, 0, Max);
        OnHealthChanged?.Invoke(Current, Max); // EI OnDamaged-kutsua
    }
    public void ForceInvulnerability(float seconds)
    {
        if (seconds <= 0) return;
        StopAllCoroutines();
        StartCoroutine(CoForceIFrames(seconds));
    }
    IEnumerator CoForceIFrames(float t) { invuln = true; yield return new WaitForSeconds(t); invuln = false; }

    public void Heal(float amount)
    {
        Current = Mathf.Min(Max, Current + amount);
        OnHealthChanged?.Invoke(Current, Max);
    }

    public void ApplyDamage(float amount, Vector2 hitDir)
    {
        if (Current <= 0) return;
        if (useInvulnerability && invuln) return;

        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, Max);

        // kerro kuittauksena että nyt sattui
        OnDamaged?.Invoke(amount, hitDir);                     // <-- UUSI

        if (applyKnockback && rb)
            rb.AddForce(hitDir * knockbackScale, ForceMode2D.Impulse);

        if (Current <= 0) { Kill(); return; }

        if (useInvulnerability) StartCoroutine(CoIFrames());
    }

    System.Collections.IEnumerator CoIFrames()
    {
        invuln = true;
        yield return new WaitForSeconds(invulnSeconds);
        invuln = false;
    }

    public void Kill()
    {
        if (Current > 0) Current = 0;
        OnDeath?.Invoke();
    }
}
