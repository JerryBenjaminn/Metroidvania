using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Patroller : EnemyBase
{
    [Header("Patrol")]
    public float speed = 2f;
    public LayerMask groundMask;      // mihin se k‰velee
    public LayerMask obstacleMask;    // sein‰t, reunat
    public Transform groundProbe;     // etuk‰rki (jalat), jos null -> k‰yt‰ omaa transformia
    public float groundProbeDistance = 0.2f; // kuinka pitk‰lle alas katsotaan
    public float wallProbeDistance = 0.1f;   // kuinka l‰hell‰ sein‰ k‰‰nt‰‰
    public bool startFacingRight = true;

    [Header("Visuals")]
    public SpriteRenderer sr;

    int dir; // -1 vasen, +1 oikea

    protected override void Start()
    {
        base.Start();
        dir = startFacingRight ? +1 : -1;
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        rb.gravityScale = rb.gravityScale <= 0 ? 1 : rb.gravityScale;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // liike
        var v = rb.linearVelocity;
        v.x = dir * speed;
        rb.linearVelocity = v;

        // k‰‰nny jos sein‰ edess‰ tai reuna allap‰in
        Vector2 origin = groundProbe ? (Vector2)groundProbe.position : (Vector2)transform.position;
        Vector2 ahead = origin + Vector2.right * dir * wallProbeDistance;
        bool wall = Physics2D.OverlapCircle(ahead, 0.05f, obstacleMask);

        bool groundAhead = Physics2D.Raycast(origin + Vector2.right * dir * 0.1f, Vector2.down, groundProbeDistance, groundMask);
        if (wall || !groundAhead)
        {
            dir *= -1;
        }

        // flippi
        if (sr) sr.flipX = dir < 0;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 origin = groundProbe ? (Vector2)groundProbe.position : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin + Vector2.right * dir * 0.1f, origin + Vector2.right * dir * 0.1f + Vector2.down * groundProbeDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin + Vector2.right * dir * wallProbeDistance, 0.05f);
    }
}
