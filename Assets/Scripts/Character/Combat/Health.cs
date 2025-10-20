using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour, IHealth, IDamageable
{
    [Header("Hearts (HK-style)")]
    [SerializeField] int maxHearts = 5;
    [SerializeField] int currentHearts = 5;

    // Legacy float-API, pidetään kompatin vuoksi (1.0 = 1 heart)
    [SerializeField] float max = 5f;
    public float Max => maxHearts;
    public float Current { get; private set; }

    [Header("Damage Response")]
    [SerializeField] bool applyKnockback = true;
    [SerializeField] float knockbackScale = 1f;
    [SerializeField] bool useInvulnerability = true;
    [SerializeField] float invulnSeconds = 0.15f;

    [Header("Audio")]
    [SerializeField] SoundEvent hurtSound;
    [SerializeField] SoundEvent deathSound;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, Vector2> OnDamaged;

    public float InvulnSeconds => invulnSeconds;
    public bool IsInvulnerable => invuln;

    Rigidbody2D rb;
    bool invuln;

    void Awake()
    {
        maxHearts = Mathf.Max(1, maxHearts <= 0 ? Mathf.RoundToInt(max) : maxHearts);
        currentHearts = Mathf.Clamp(currentHearts <= 0 ? maxHearts : currentHearts, 0, maxHearts);
        max = maxHearts;
        Current = currentHearts;
        rb = GetComponent<Rigidbody2D>();
        OnHealthChanged?.Invoke(Current, Max);
    }

    public int MaxHearts => maxHearts;
    public int CurrentHearts => currentHearts;

    public void SetMaxHearts(int value, bool healToFull = true)
    {
        maxHearts = Mathf.Max(1, value);
        if (healToFull) currentHearts = maxHearts;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);
        max = maxHearts;
        Current = currentHearts;
        OnHealthChanged?.Invoke(Current, Max);
    }

    public void SetHealthFromSave(float value)
    {
        currentHearts = Mathf.Clamp(Mathf.RoundToInt(value), 0, maxHearts);
        Current = currentHearts;
        OnHealthChanged?.Invoke(Current, Max);
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
        int add = Mathf.Max(0, Mathf.RoundToInt(amount));
        if (add == 0 || currentHearts <= 0) return;
        currentHearts = Mathf.Clamp(currentHearts + add, 0, maxHearts);
        Current = currentHearts;
        OnHealthChanged?.Invoke(Current, Max);
    }

    public void ApplyDamage(float amount, Vector2 hitDir)
    {
        if (currentHearts <= 0) return;
        if (useInvulnerability && invuln) return;

        int dmg = Mathf.Max(0, Mathf.RoundToInt(amount));
        if (dmg == 0) return;

        currentHearts = Mathf.Max(0, currentHearts - dmg);
        Current = currentHearts;
        OnHealthChanged?.Invoke(Current, Max);
        OnDamaged?.Invoke(dmg, hitDir);

        if (hurtSound != null)
        {
            AudioManager.Instance.Play(hurtSound, transform.position, is2D: true);
        }

        if (applyKnockback && rb) rb.AddForce(hitDir * knockbackScale, ForceMode2D.Impulse);

        if (currentHearts <= 0) { Kill(); return; }
        if (useInvulnerability) StartCoroutine(CoIFrames());
    }
    public void SetHurtSound(SoundEvent sound)
    {
        hurtSound = sound;
    }

    IEnumerator CoIFrames() { invuln = true; yield return new WaitForSeconds(invulnSeconds); invuln = false; }

    public void Kill()
    {
        if (currentHearts > 0) currentHearts = 0;
        Current = currentHearts;
        OnHealthChanged?.Invoke(Current, Max);

        if (deathSound != null)
        {
            AudioManager.Instance.Play(deathSound, transform.position, is2D: true);
        }

        OnDeath?.Invoke();
    }
    public void SetDeathSound(SoundEvent sound)
    {
        deathSound = sound;
    }
}
