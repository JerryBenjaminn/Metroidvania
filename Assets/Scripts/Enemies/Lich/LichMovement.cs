using UnityEngine;
using System.Collections;

public class LichMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform[] waypoints;
    public float waypointPauseDuration = 2f;
    public float teleportChance = 0.3f;

    [Header("Facing")]
    public Transform player;
    public SpriteRenderer sprite;

    [Header("Attacks")]
    public GameObject lightningPrefab; // Prefab for the lightning attack
    public GameObject patrollerPrefab; // Prefab for the summoned patroller
    public GameObject homingProjectilePrefab; // Prefab for the homing projectile
    public GameObject flyerPrefab; //Prefab for the summoned flyer
    public Transform attackSpawnPoint; // Spawn point for attacks
    public float homingProjectileLifetime = 3f; // Lifetime of homing projectiles
    public int[] groundWaypointIndices; // Indices of ground waypoints

    [Header("Second Phase")]
    public bool isSecondPhase = false; // Tracks if the Lich is in the second phase
    public float secondPhaseHealthThreshold = 0.6f; // 60% health threshold
    public float secondPhaseAttackSpeedMultiplier = 1.5f; // Multiplier for attack speed in the second phase
    public GameObject lightningStormPrefab; // Prefab for the lightning storm attack
    public Transform[] lightningStormPositions; // Positions for the lightning storm

    public Animator animator;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isAttacking = false;
    private bool isMovingLeg = false;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to LichMovement!");
        }
    }

    void Update()
    {
        FlipTowardsPlayer();

        if (!isSecondPhase && GetComponent<Health>().Current / GetComponent<Health>().Max <= secondPhaseHealthThreshold)
        {
            ActivateSecondPhase();
        }
    }

    void FixedUpdate()
    {
        if (!isWaiting && !isAttacking)
        {
            if (isMovingLeg)
            {
                MoveToWaypoint();
            }
            else
            {
                StartNextLeg();
            }
        }
    }

    void ActivateSecondPhase()
    {
        isSecondPhase = true;
        Debug.Log("Lich has entered the second phase!");

        // Increase attack speed
        waypointPauseDuration /= secondPhaseAttackSpeedMultiplier;

        // Optionally, add visual or audio effects to indicate the phase change
        animator.SetTrigger("SecondPhase");
    }

    void MoveToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector2 targetPos = waypoints[currentWaypointIndex].position;
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPos) < 0.05f)
        {
            isMovingLeg = false;
            StartCoroutine(PauseAtWaypoint());
        }
    }
    void TeleportToWaypoint(int newWaypointIndex)
    {
        if (isWaiting) return;
        isWaiting = true;
        StartCoroutine(TeleportOutAnimation());
        currentWaypointIndex = newWaypointIndex;
        rb.position = waypoints[currentWaypointIndex].position; // älä käytä transform.position
        Debug.Log($"Lich teleported to waypoint {currentWaypointIndex}");
        StartCoroutine(TeleportInAnimation());
        StartCoroutine(PauseAtWaypoint());
    }
    IEnumerator TeleportOutAnimation()
    {
        animator.SetTrigger("TeleportOut");
        yield return new WaitForSeconds(0.25f);
    }
    IEnumerator TeleportInAnimation()
    {
        animator.SetTrigger("TeleportIn");
        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger("Idle");
    }

    IEnumerator PauseAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waypointPauseDuration);

        // Hyökkäys
        yield return StartCoroutine(PerformAttack());

        isWaiting = false;

        // Päätä seuraava legi (teleport vai kävely)
        StartNextLeg();
    }

    void StartNextLeg()
    {

        if (waypoints == null || waypoints.Length == 0) return;

        // Select the next waypoint index (different from the current one)
        int nextIndex;
        if (waypoints.Length == 1)
        {
            nextIndex = 0;
        }
        else
        {
            do
            {
                nextIndex = Random.Range(0, waypoints.Length);
            } while (nextIndex == currentWaypointIndex);
        }

        // Decide whether to teleport or move
        if (Random.value < teleportChance)
        {
            TeleportToWaypoint(nextIndex);
        }
        else
        {
            currentWaypointIndex = nextIndex;
            isMovingLeg = true; // Set the flag to start moving
        }
    }
    IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Create a list of valid attacks based on the Lich's current state
        var validAttacks = new System.Collections.Generic.List<int>();

        if (IsOnGroundWaypoint())
        {
            validAttacks.Add(1); // Lightning Attack
            validAttacks.Add(2); // Summon Patroller
        }
        else
        {
            validAttacks.Add(4); // Summon Flyer

            if (isSecondPhase)
            {
                validAttacks.Add(5); // Lightning Storm
            }
        }

        validAttacks.Add(3); // Homing Projectile (always valid)



        // Randomly choose one of the valid attacks
        int attackIndex = validAttacks[Random.Range(0, validAttacks.Count)];
        Debug.Log($"Lich is attempting attack {attackIndex}");

        switch (attackIndex)
        {
            case 1:
                Debug.Log("Performing Lightning Attack anim");
                animator.SetTrigger("LightningAttack");
                yield return new WaitForSeconds(1.3f);
                PerformLightningAttack();
                animator.SetTrigger("Idle");
                break;

            case 2:
                Debug.Log("Performing Summon Patroller anim");
                animator.SetTrigger("SummonPatroller");
                yield return new WaitForSeconds(2f);
                PerformSummonPatroller();
                animator.SetTrigger("Idle");
                break;

            case 3:
                Debug.Log("Performing Homing Projectile Attack anim");
                animator.SetTrigger("HomingProjectile");
                yield return new WaitForSeconds(0.25f);
                yield return StartCoroutine(PerformHomingProjectileAttack());
                animator.SetTrigger("Idle");
                break;

            case 4:
                Debug.Log("Performing Summon Flyer anim");
                animator.SetTrigger("SummonPatroller");
                yield return new WaitForSeconds(2f);
                PerformSummonFlyer();
                animator.SetTrigger("Idle");
                break;

            case 5: // Lightning Storm
                Debug.Log("Performing Lightning Storm attack");
                animator.SetTrigger("LightningStorm");
                yield return StartCoroutine(PerformLightningStorm());
                animator.SetTrigger("Idle");
                break;
        }

        // Wait for the attack animation or delay (adjust as needed)
        yield return new WaitForSeconds(1f);

        isAttacking = false;
    }

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

    void PerformSummonFlyer()
    {
        if (flyerPrefab && attackSpawnPoint)
        {
            Instantiate(flyerPrefab, attackSpawnPoint.position, Quaternion.identity);
            Debug.Log("Lich summoned a Flyer!");
        }
    }

    IEnumerator PerformHomingProjectileAttack()
    {
        if (!homingProjectilePrefab || !attackSpawnPoint) yield break;

        // Randomly determine the number of projectiles to shoot (2 to 3)
        int projectileCount = Random.Range(2, 4);

        if (isSecondPhase)
        {
            projectileCount = Random.Range(3, 5);
        }

        Debug.Log($"Lich will shoot {projectileCount} homing projectiles!");

        for (int i = 0; i < projectileCount; i++)
        {
            // Instantiate the homing projectile
            var go = Instantiate(homingProjectilePrefab, attackSpawnPoint.position, Quaternion.identity);
            var homing = go.GetComponent<HomingProjectile>();
            if (homing)
            {
                homing.SetLifetime(homingProjectileLifetime);
                homing.Init(player);
            }

            // Wait for 0.5 seconds before spawning the next projectile
            yield return new WaitForSeconds(0.75f);
        }
    }
    IEnumerator PerformLightningStorm()
    {
        if (lightningStormPrefab == null || lightningStormPositions == null || lightningStormPositions.Length == 0)
        {
            Debug.LogWarning("Lightning Storm setup is incomplete!");
            yield break;
        }

        foreach (Transform position in lightningStormPositions)
        {
            // Instantiate a lightning strike at each position
            Instantiate(lightningStormPrefab, position.position, Quaternion.Euler(0, 0, -90));
            Debug.Log($"Lightning strike at {position.position}");

            // Wait briefly before the next strike
            yield return new WaitForSeconds(2.5f / secondPhaseAttackSpeedMultiplier);
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