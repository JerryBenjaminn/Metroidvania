using UnityEngine;

public class EnemyAnimEventRelay : MonoBehaviour
{
    // Kutsu tätä Animation Eventistä
    public void Animation_DestroySelf()
    {
        var enemy = GetComponentInParent<EnemyBase>();
        if (enemy) Destroy(enemy.gameObject);
        else Destroy(gameObject);
    }
}
