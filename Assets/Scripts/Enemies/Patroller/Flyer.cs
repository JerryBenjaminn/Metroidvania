using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Flyer : EnemyBase
{
    [Header("Targeting")]
    public float detectRange = 7f;
    public LayerMask losObstacles; // Ground tms
    public string playerTag = "Player";

    [Header("Motion")]
    public float hoverAmplitude = 0.3f;
    public float hoverFreq = 1.5f;
    public float maxSpeed = 5f;
    public float steer = 4f;           // kuinka nopeasti k��ntyy kohti tavoitetta
    public float swoopInterval = 1.2f; // kuinka usein "hy�kk��"
    public float swoopBoost = 1.4f;    // nopeuskerroin swoopin alussa
    public float drag = 0.2f;          // pieni ilmanvastus pehment��

    [Header("Visuals")]
    public SpriteRenderer sr;

    Transform player;
    Vector2 homePos;
    float t;
    float nextSwoopAt;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = drag;
        homePos = transform.position;
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        var pObj = GameObject.FindGameObjectWithTag(playerTag);
        if (pObj) player = pObj.transform;
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;

        Vector2 target = HoverTarget();
        bool seesPlayer = false;

        if (player)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= detectRange && HasLoS((Vector2)transform.position, (Vector2)player.position))
            {
                // kohde on pelaaja
                target = (Vector2)player.position;

                // swoop: pieni nopeuspiikki s��nn�llisin v�lein
                if (Time.time >= nextSwoopAt)
                {
                    nextSwoopAt = Time.time + swoopInterval;
                    // hetkellinen "boost" antamaan kaaren tunnetta
                    rb.linearVelocity *= swoopBoost;
                }
                seesPlayer = true;
            }
        }

        // ohjaus kohti targetia pehme�sti kaartava reitti
        Vector2 desired = (target - (Vector2)transform.position).normalized * maxSpeed;
        Vector2 v = rb.linearVelocity;
        v = Vector2.Lerp(v, desired, steer * Time.fixedDeltaTime);
        rb.linearVelocity = v;

        // flip
        if (sr && Mathf.Abs(v.x) > 0.01f) sr.flipX = v.x < 0f;

        //OnMovement?.Invoke(rb.linearVelocity, false);
    }

    Vector2 HoverTarget()
    {
        // leiju alkuper�isen kotiposition ymp�rill� pienen sinik�yr�n mukaan
        var offset = new Vector2(0f, Mathf.Sin(t * hoverFreq) * hoverAmplitude);
        return homePos + offset;
    }

    bool HasLoS(Vector2 from, Vector2 to)
    {
        var hit = Physics2D.Linecast(from, to, losObstacles);
        return hit.collider == null;
    }
}
