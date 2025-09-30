using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody2D))]
public abstract class EnemyBase : ActorBase
{
    protected Health health;
    [SerializeField] protected float contactDamage = 10f;

    protected virtual void Start() {
        health = GetComponent<Health>();
        health.OnDeath += HandleDeath;
    }

    protected virtual void OnDestroy() {
        if (health != null) health.OnDeath -= HandleDeath;
    }

    protected virtual void HandleDeath() {
        // dropit, animaatiot, poolaus
        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {
        if (other.collider.TryGetComponent<IDamageable>(out var dmg))
            dmg.ApplyDamage(contactDamage, (other.transform.position - transform.position).normalized);
    }
}

