using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HomingProjectile : MonoBehaviour
{
    public enum ForwardAxis { Up, Right }

    [Header("Setup")]
    public ForwardAxis forwardAxis = ForwardAxis.Up; // Mihin suuntaan sprite on piirretty
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

    // Kutsutaan heti spawnin j�lkeen
    public void Init(Transform t)
    {
        target = t;

        // Suunta pelaajaan spawnihetkell�
        Vector2 toTarget = (target ? (Vector2)(target.position - transform.position) : Vector2.up).normalized;

        // Aseta alkurotatio sen mukaan mihin suuntaan sprite "osoittaa"
        float angleDeg = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        if (forwardAxis == ForwardAxis.Up) angleDeg -= 90f; // koska up-etu
        rb.rotation = angleDeg;

        // L�hde heti suoraan kohti pelaajaa
        rb.linearVelocity = toTarget * speed;
    }

    void FixedUpdate()
    {
        // Elinik�
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            // Ei kohdetta, jatka suoraan
            rb.linearVelocity = GetForward() * speed;
            return;
        }

        // Tuhoudu jos ollaan tarpeeksi l�hell�
        Vector2 toTarget = (Vector2)(target.position - transform.position);
        if (toTarget.magnitude <= stopDistance)
        {
            Destroy(gameObject);
            return;
        }

        // HOMING
        if (homingTimer > 0f)
        {
            homingTimer -= Time.fixedDeltaTime;

            Vector2 currentForward = GetForward();
            Vector2 desiredForward = toTarget.normalized;

            // K��nn� pehme�sti kohti kohdetta
            float maxRad = turnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime;
            Vector2 newForward = Vector3.RotateTowards(currentForward, desiredForward, maxRad, 0f);

            // P�ivit� rotatio newForwardin mukaan
            float newAngle = Mathf.Atan2(newForward.y, newForward.x) * Mathf.Rad2Deg;
            if (forwardAxis == ForwardAxis.Up) newAngle -= 90f;
            rb.MoveRotation(newAngle);
        }

        // Kulje aina eteenp�in nykyisen etusuunnan mukaan
        rb.linearVelocity = GetForward() * speed;
    }

    private Vector2 GetForward()
    {
        // Huom: transform.up/transform.right p�ivittyy rb.rotationista
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
            // damage tms.
            Destroy(gameObject);
        }
    }
}
