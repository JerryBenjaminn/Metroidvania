using UnityEngine;
using System.Collections;

public class LichMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform[] waypoints;
    public float waypointPauseDuration = 2f;

    [Header("Facing")]
    public Transform player;
    public SpriteRenderer sprite;

    [Header("Attacks")]
    public GameObject lightningPrefab; // Prefab for the lightning attack
    public GameObject patrollerPrefab; // Prefab for the summoned patroller
    public GameObject homingProjectilePrefab; // Prefab for the homing projectile
    public Transform attackSpawnPoint; // Spawn point for attacks
    public float homingProjectileLifetime = 3f; // Lifetime of homing projectiles
    public int[] groundWaypointIndices; // Indices of ground waypoints

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isAttacking = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to LichMovement!");
        }
    }

    void Update()
    {
        FlipTowardsPlayer();
    }

    void FixedUpdate()
    {
        if (!isWaiting && !isAttacking) MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector2 targetPos = waypoints[currentWaypointIndex].position;
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPos) < 0.1f)
            StartCoroutine(PauseAtWaypoint());
    }

    IEnumerator PauseAtWaypoint()
    {
        isWaiting = true;

        // Wait for the pause duration
        yield return new WaitForSeconds(waypointPauseDuration);

        // Perform an attack
        StartCoroutine(PerformAttack());

        // Move to the next waypoint
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        isWaiting = false;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Randomly choose one of the three attacks
        int attackIndex = Random.Range(1, 4);

        switch (attackIndex)
        {
            case 1:
                if (IsOnGroundWaypoint())
                {
                    PerformLightningAttack();
                }
                break;
            case 2:
                PerformSummonPatroller();
                break;
            case 3:
                PerformHomingProjectileAttack();
                break;
        }

        // Wait for the attack animation or delay (adjust as needed)
        yield return new WaitForSeconds(1f);

        isAttacking = false;
    }

    // 10/9/2025 AI-Tag
    // This was created with the help of Assistant, a Unity Artificial Intelligence product.

    void PerformLightningAttack()
    {
        if (lightningPrefab && attackSpawnPoint)
        {
            // Determine the direction the Lich is facing
            float facingDirection = sprite.flipX ? -1f : 1f;

            // Set the rotation of the lightning based on the facing direction
            Quaternion rotation = Quaternion.Euler(0, facingDirection < 0 ? 180f : 0, 0);

            // Instantiate the lightning prefab with the correct rotation
            GameObject lightning = Instantiate(lightningPrefab, attackSpawnPoint.position, rotation);

            // Destroy the lightning object after a short delay
            Destroy(lightning, 0.33f);

            Debug.Log("Lich performed Lightning Attack!");
        }
    }

    void PerformSummonPatroller()
    {
        if (patrollerPrefab && attackSpawnPoint)
        {
            Instantiate(patrollerPrefab, attackSpawnPoint.position, Quaternion.identity);
            Debug.Log("Lich summoned a Patroller!");
        }
    }

    void PerformHomingProjectileAttack()
    {
        if (!homingProjectilePrefab || !attackSpawnPoint) return;

        var go = Instantiate(homingProjectilePrefab, attackSpawnPoint.position, Quaternion.identity);
        var homing = go.GetComponent<HomingProjectile>();
        if (homing)
        {
            homing.SetLifetime(homingProjectileLifetime);
            homing.Init(player);
        }
    }

    void FlipTowardsPlayer()
    {
        if (!player || !sprite || !attackSpawnPoint) return;

        // Determine the direction to the player
        float dx = player.position.x - transform.position.x;

        // Flip the sprite
        bool shouldFlip = dx > 0f;
        sprite.flipX = shouldFlip;

        // Adjust the attackSpawnPoint's local position
        Vector3 localPosition = attackSpawnPoint.localPosition;
        localPosition.x = Mathf.Abs(localPosition.x) * (sprite.flipX ? -1 : 1);
        attackSpawnPoint.localPosition = localPosition;
    }

    bool IsOnGroundWaypoint()
    {
        foreach (int index in groundWaypointIndices)
        {
            if (currentWaypointIndex == index)
            {
                return true;
            }
        }
        return false;
    }
}