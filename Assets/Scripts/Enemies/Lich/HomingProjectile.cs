using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Homing Settings")]
    public float speed = 6f;                 // liike-etenemisnopeus
    public float turnRateDegPerSec = 360f;   // maksimik‰‰ntˆnopeus asteina/s
    public float stopDistance = 0.5f;        // tuhoa kun p‰‰st‰‰n tarpeeksi l‰helle
    public float homingDuration = 0.6f;      // kuinka kauan haetaan kohdetta
    public float maxLifetime = 3f;           // kovakatto elini‰lle

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

    void FixedUpdate()
    {
        // Elinik‰ aina tikitt‰‰
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Jos ei targettia, ved‰ suoraan eteenp‰in
        if (target == null)
        {
            rb.linearVelocity = transform.up * speed;
            return;
        }

        // Jos tarpeeksi l‰hell‰, tuhoa
        Vector2 toTarget = (Vector2)(target.position - transform.position);
        float dist = toTarget.magnitude;
        if (dist <= stopDistance)
        {
            Destroy(gameObject);
            return;
        }

        // HOMING-vaihe
        if (homingTimer > 0f)
        {
            homingTimer -= Time.fixedDeltaTime;

            // haluttu kulma kohti targettia
            float desiredDeg = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f; // -90 jos ohjus "osoittaa ylˆs"
            float currentDeg = rb.rotation;
            float newDeg = Mathf.MoveTowardsAngle(currentDeg, desiredDeg, turnRateDegPerSec * Time.fixedDeltaTime);
            rb.rotation = newDeg;
        }

        // eteneminen aina eteenp‰in (oli homing p‰‰ll‰ tai ei)
        rb.linearVelocity = transform.up * speed;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    // Yhteensopivuus vanhan kutsun kanssa: k‰yt‰ t‰t‰ max-lifetimeena
    public void SetLifetime(float newLifetime)
    {
        maxLifetime = newLifetime;
        lifeTimer = maxLifetime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // damage t‰h‰n jos haluat
            Destroy(gameObject);
        }
    }
}
