using UnityEngine;
using System;

public class Health : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] float max = 100;
    public float Max => max;
    public float Current { get; private set; }

    public event Action OnDeath;
    public event Action<float,float> OnHealthChanged; // current, max

    void Awake() => Current = max;

    public void Heal(float amount) {
        Current = Mathf.Min(Max, Current + amount);
        OnHealthChanged?.Invoke(Current, Max);
    }

    public void ApplyDamage(float amount, Vector2 hitDir) {
        if (Current <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, Max);
        if (Current <= 0) Kill();
    }

    public void Kill() {
        if (Current > 0) Current = 0;
        OnDeath?.Invoke();
    }
}
