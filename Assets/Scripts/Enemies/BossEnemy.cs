// 10/8/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class BossEnemy : EnemyBase
{
    private bool isActive = false;

    public void ActivateBoss()
    {
        isActive = true;
        // Play boss intro animation
        if (animator != null)
        {
            animator.SetTrigger("BossIntro");
        }
    }

    protected override void HandleDeath()
    {
        base.HandleDeath();
        // Notify the arena that the boss is defeated
        FindFirstObjectByType<BossArena>()?.EndBossBattle();
    }

    private void Update()
    {
        if (!isActive) return;

        // Boss logic goes here (e.g., state machine updates)
    }
}