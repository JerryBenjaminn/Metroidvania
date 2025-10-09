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
        yield return new WaitForSeconds(waypointPauseDuration);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        isWaiting = false;
    }

    void FlipTowardsPlayer()
    {
        if (!player || !sprite) return;

        float dx = player.position.x - transform.position.x;

        sprite.flipX = dx > 0f;
    }
}
