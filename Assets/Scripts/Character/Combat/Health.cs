using UnityEngine;
using System;

public class Health : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] float max = 100;
    public float Max => max;
    public float Current { get; private set; }

    [Header("Damage Response")]
    [SerializeField] bool applyKnockback = true;
    [SerializeField] float knockbackScale = 1f;         // skaalaa kontaktista tulevaa voimaa
    [SerializeField] bool useInvulnerability = true;
    [SerializeField] float invulnSeconds = 0.15f;       // pieni i-frame ettei ContactDamage tikit‰ liian tihe‰‰n

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // current, max

    Rigidbody2D rb;
    bool invuln;

    void Awake()
    {
        Current = max;
        rb = GetComponent<Rigidbody2D>();
    }

    public void Heal(float amount)
    {
        Current = Mathf.Min(Max, Current + amount);
        OnHealthChanged?.Invoke(Current, Max);
    }

    // IDamageable
    public void ApplyDamage(float amount, Vector2 hitDir)
    {
        if (Current <= 0) return;
        if (useInvulnerability && invuln) return;

        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, Max);

        if (applyKnockback && rb)
        {
            // ContactDamage antaa jo valmiiksi suunta * voimakkuus skaalataan vain
            rb.AddForce(hitDir * knockbackScale, ForceMode2D.Impulse);
        }

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
