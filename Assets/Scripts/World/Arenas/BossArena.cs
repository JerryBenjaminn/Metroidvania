using UnityEngine;
using System.Collections;

public class BossArena : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject arenaDoors; // Doors to close the arena
    [SerializeField] private Animator arenaAnimator; // Animator for arena animations
    [SerializeField] private LichMovement lich; // Reference to the LichMovement script
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

        // Activate the Lich
        if (lich != null)
        {
            lich.player = player; // Assign the player to the Lich
            lich.enabled = true; // Enable the LichMovement script
            lich.animator.SetTrigger("Intro"); // Play the intro animation

            // Wait for the intro animation to finish, then set Idle
            StartCoroutine(TransitionToIdle());
        }
    }

    private IEnumerator TransitionToIdle()
    {
        yield return new WaitForSeconds(1.3f); // Adjust this duration to match the intro animation length
        lich.animator.SetTrigger("Idle");
    }

    public void EndBossBattle()
    {
        // Open arena doors after the boss is defeated
        if (arenaDoors != null)
        {
            arenaDoors.SetActive(false);
        }
    }
    public void ResetBossFight()
    {
        // Reset the battle state
        battleStarted = false;

        // Open the arena doors
        if (arenaDoors != null)
        {
            arenaDoors.SetActive(false);
        }

        // Reset the Lich
        if (lich != null)
        {
            lich.enabled = false; // Disable the LichMovement script
            lich.isSecondPhase = false; // Reset the second phase
            lich.transform.position = lich.waypoints[1].position; // Reset to the initial waypoint

            // Reset the Lich's health
            Health lichHealth = lich.GetComponent<Health>();
            if (lichHealth != null)
            {
                lichHealth.Heal(lichHealth.Max); // Reset health to maximum
            }

            // Trigger a teleportation sequence to reset animations
            StartCoroutine(TeleportLichAfterReset());
        }

        // Optionally reset the arena animator
        if (arenaAnimator != null)
        {
            arenaAnimator.ResetTrigger("StartBattle");
        }
    }

    private IEnumerator TeleportLichAfterReset()
    {
        // Wait briefly before teleporting
        yield return new WaitForSeconds(0.1f);

        // Trigger teleportation
        lich.animator.SetTrigger("TeleportOut");
        yield return new WaitForSeconds(0.25f); // Match the teleport out animation duration

        lich.transform.position = lich.waypoints[1].position; // Ensure it's at the correct waypoint
        lich.animator.SetTrigger("TeleportIn");
        yield return new WaitForSeconds(0.3f); // Match the teleport in animation duration

        // Set the Lich to Idle state
        lich.animator.SetTrigger("Idle");

        // Re-enable the LichMovement script
        //lich.enabled = true;
    }
}