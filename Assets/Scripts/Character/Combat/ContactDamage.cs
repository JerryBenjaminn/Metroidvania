using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ContactDamage : MonoBehaviour
{
    public float damage = 10f;
    public float knockback = 6f;
    public float tickCooldown = 0.35f;
    public LayerMask targetMask; // esim. Player-layer

    // pieni per-kohde cooldown, ettei dps ole tyhmä
    Dictionary<GameObject, float> lastHitAt = new();

    void TryHit(GameObject other, Vector2 fromPos)
    {
        if (other == null) return;
        if (((1 << other.layer) & targetMask) == 0) return;

        if (!lastHitAt.TryGetValue(other, out var t) || Time.time >= t + tickCooldown)
        {
            lastHitAt[other] = Time.time;
            if (other.TryGetComponent<IDamageable>(out var dmg))
            {
                var dir = (other.transform.position - (Vector3)fromPos).normalized;
                dmg.ApplyDamage(damage, dir * knockback);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D c) => TryHit(c.collider.gameObject, transform.position);
    void OnCollisionStay2D(Collision2D c) => TryHit(c.collider.gameObject, transform.position);
    void OnTriggerEnter2D(Collider2D c) => TryHit(c.gameObject, transform.position);
    void OnTriggerStay2D(Collider2D c) => TryHit(c.gameObject, transform.position);
}

