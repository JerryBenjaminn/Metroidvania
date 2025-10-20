using UnityEngine;
using System.Collections;
using System.Linq;

public class BossArenaTrigger : MonoBehaviour
{
    public GameObject lichPrefab; // Prefab of the Lich boss
    public Transform lichSpawnPoint; // Spawn point for the Lich
    public GameObject arenaBoundary; // Optional: Activate arena boundaries when the fight starts

    private bool bossFightStarted = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger
        if (other.CompareTag("Player") && !bossFightStarted)
        {
            bossFightStarted = true;
            StartCoroutine(StartBossFight());
        }
    }

    IEnumerator StartBossFight()
    {
        // Instantiate the Lich at the spawn point
        GameObject lich = Instantiate(lichPrefab, lichSpawnPoint.position, Quaternion.identity);

        // Find the player in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene! Make sure the Player has the 'Player' tag.");
            yield break;
        }

        // Assign the player to the LichMovement script
        LichMovement lichMovement = lich.GetComponent<LichMovement>();
        lichMovement.player = player.transform;

        // Find all waypoints in the scene and assign them
        Transform[] waypoints = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Where(t => t.CompareTag("Waypoint")).ToArray();
        //lichMovement.SetWaypoints(waypoints);

        // Assign lightning storm positions dynamically
        Transform[] lightningStormPositions = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Where(t => t.CompareTag("LightningStormPosition")).ToArray();
        lichMovement.lightningStormPositions = lightningStormPositions;

        // Play the intro animation
        Animator lichAnimator = lich.GetComponentInChildren<Animator>();

        lichAnimator.SetTrigger("Intro");

        // Play intro sound
        if (lichMovement.introSound != null)
        {
            AudioManager.Instance.Play(lichMovement.introSound, transform.position, is2D: true);
        }


        // Wait for the intro animation to finish
        yield return new WaitForSeconds(1.5f);

        // Enable the Lich's movement script
        lichMovement.enabled = true;

        Debug.Log("Boss fight started!");
    }
}