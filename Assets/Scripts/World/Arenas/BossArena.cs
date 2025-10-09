using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject arenaDoors; // Doors to close the arena
    [SerializeField] private Animator arenaAnimator; // Animator for arena animations
    [SerializeField] private BossEnemy boss; // Reference to the boss
    [SerializeField] private Transform player; // Reference to the player

    private bool battleStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            StartBossBattle();
        }
    }

    private void StartBossBattle()
    {
        // Close arena doors
        if (arenaDoors != null)
        {
            arenaDoors.SetActive(true);
        }

        // Play arena intro animation
        if (arenaAnimator != null)
        {
            arenaAnimator.SetTrigger("StartBattle");
        }

        // Activate the boss
        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }

    public void EndBossBattle()
    {
        // Open arena doors after the boss is defeated
        if (arenaDoors != null)
        {
            arenaDoors.SetActive(false);
        }
    }
}