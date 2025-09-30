using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float damage = 10;
    public Vector2 pushDir = Vector2.right;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<IDamageable>(out var dmg)) {
            var dir = (other.transform.position - transform.position).normalized;
            dmg.ApplyDamage(damage, dir);
        }
    }
}

