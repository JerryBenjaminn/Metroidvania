// 10/9/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HomingProjectile : MonoBehaviour
{
    public enum ForwardAxis { Up, Right }

    [Header("Setup")]
    public ForwardAxis forwardAxis = ForwardAxis.Up; // Direction the sprite is drawn
    public float speed = 6f;
    public float turnRateDegPerSec = 360f;
    public float homingDuration = 0.6f;
    public float maxLifetime = 3f;
    public float stopDistance = 0.5f;

    private Transform target;
    private Rigidbody2D rb;
    private float homingTimer;
    private float lifeTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homingTimer = homingDuration;
        lifeTimer = maxLifetime;
    }

    public void Init(Transform t)
    {
        target = t;

        // Initial direction toward the player
        Vector2 toTarget = (target ? (Vector2)(target.position - transform.position) : Vector2.up).normalized;

        // Set initial rotation based on sprite's forward axis
        float angleDeg = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        if (forwardAxis == ForwardAxis.Up) angleDeg -= 90f;
        rb.rotation = angleDeg;

        // Start moving toward the player
        rb.linearVelocity = toTarget * speed;
    }

    void FixedUpdate()
    {
        // Lifetime management
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            // Continue straight if no target
            rb.linearVelocity = GetForward() * speed;
            return;
        }

        // Destroy if close enough to the target
        Vector2 toTarget = (Vector2)(target.position - transform.position);


        // Homing logic
        if (homingTimer > 0f)
        {
            homingTimer -= Time.fixedDeltaTime;

            Vector2 currentForward = GetForward();
            Vector2 desiredForward = toTarget.normalized;

            // Smoothly rotate toward the target
            float maxRad = turnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime;
            Vector2 newForward = Vector3.RotateTowards(currentForward, desiredForward, maxRad, 0f);

            // Update rotation based on new forward direction
            float newAngle = Mathf.Atan2(newForward.y, newForward.x) * Mathf.Rad2Deg;
            if (forwardAxis == ForwardAxis.Up) newAngle -= 90f;
            rb.MoveRotation(newAngle);
        }

        // Move forward in the current direction
        rb.linearVelocity = GetForward() * speed;
    }

    private Vector2 GetForward()
    {
        return forwardAxis == ForwardAxis.Up ? (Vector2)transform.up : (Vector2)transform.right;
    }

    public void SetLifetime(float seconds)
    {
        maxLifetime = seconds;
        lifeTimer = maxLifetime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Let ContactDamage handle the damage application
            Debug.Log($"HomingProjectile collided with: {other.name}");

            // Destroy the projectile after collision
            Destroy(gameObject);
        }
    }
}