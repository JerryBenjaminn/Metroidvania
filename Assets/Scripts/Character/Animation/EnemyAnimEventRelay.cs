using UnityEngine;

public class EnemyAnimEventRelay : MonoBehaviour
{
    // Kutsu t�t� Animation Eventist�
    public void Animation_DestroySelf()
    {
        var enemy = GetComponentInParent<EnemyBase>();
        if (enemy) Destroy(enemy.gameObject);
        else Destroy(gameObject);
    }
}
