using UnityEngine;
using System;

public class CharacterMotor : ActorBase
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float accel = 60f;
    public float decel = 80f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxAirJumps = 0;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.1f;
    public Transform groundCheck;

    [Header("Gravity & Jump Tuning")]
    public float baseGravityScale = 4f;          // Rigidbody2D:n perus gravityScale
    public float fallGravityMultiplier = 1.8f;   // putoamisvaihe, napakampi pudotus
    public float lowJumpGravityMultiplier = 2.5f;// nappi irti nousussa -> matalampi hyppy
    public float riseGravityMultiplier = 1.0f;   // nousun lisäpaino (yleensä 1.0–1.1)

    [Header("Jump Leniency")]
    public float coyoteTime = 0.10f;             // reunan jälkeen sallittu hyppy
    public float jumpBufferTime = 0.10f;         // puskurointi ennen maahanosumaa

    // Sisäiset laskurit
    float coyoteCounter;
    float jumpBufferCounter;
    bool jumpHeld;                               // onko hyppynappi pohjassa

    public Vector2 Velocity => rb.linearVelocity;
    public bool IsGrounded { get; private set; }
    public int AirJumpsUsed { get; private set; }

    float targetX;

    public int Facing { get; private set; } = 1;
    public void SetMove(float x)
    {
        targetX = Mathf.Clamp(x, -1f, 1f);
        // päivitä facing vain kun syöte on oikeasti suuntainen
        if (Mathf.Abs(targetX) > 0.1f)
            Facing = targetX > 0 ? 1 : -1;
    }

    // Vanhan API:n yhteensopivuus: jos joku kutsuu tätä, ohjaa puskuriin
    public void RequestJump() => QueueJump();

    protected override void Awake() {
        base.Awake();
        rb.gravityScale = baseGravityScale;      // peruspaino päälle
    }

    void FixedUpdate() {
        // 1) Ground check
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        // 2) Coyote & buffer -laskurit
        if (IsGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.fixedDeltaTime;

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.fixedDeltaTime;

        // 3) Vaakaliike
        float desired = targetX * moveSpeed;
        float diff = desired - rb.linearVelocity.x;
        float a = Mathf.Abs(desired) > 0.01f ? accel : decel;
        float ax = Mathf.Clamp(diff * a, -a, a);
        rb.AddForce(new Vector2(ax, 0), ForceMode2D.Force);

        // 4) Hyppy puskuroinnilla + coyote + ilmahyppy
        bool canJumpNow = (IsGrounded || coyoteCounter > 0f || AirJumpsUsed < maxAirJumps);
        if (jumpBufferCounter > 0f && canJumpNow)
        {
            var v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;

            if (!IsGrounded) AirJumpsUsed++;
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;

            OnJump?.Invoke();
        }

        // 5) Dynaaminen painovoima variable jumpiin
        float velY = rb.linearVelocity.y;
        float gravityMult = 1f;

        if (velY > 0.01f) {
            // Noustessa: jos nappi irti → leikkaa hyppyä
            gravityMult = jumpHeld ? riseGravityMultiplier : lowJumpGravityMultiplier;
        } else if (velY < -0.01f) {
            // Laskiessa lisää painoa
            gravityMult = fallGravityMultiplier;
        } else {
            gravityMult = 1f;
        }

        rb.gravityScale = baseGravityScale * gravityMult;

        // 6) Ilmahyppyjen resetointi maassa
        if (IsGrounded) AirJumpsUsed = 0;

        // Jos ei syötettä ja nopeus lähes nolla, napsauta x-velocity täsmälleen nollaan
        if (Mathf.Abs(targetX) < 0.001f && Mathf.Abs(rb.linearVelocity.x) < 0.05f)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);


        // 7) Event ulos animaatiolle
        OnMovement?.Invoke(rb.linearVelocity, IsGrounded);
    }

    public event Action<Vector2,bool> OnMovement; // velocity, grounded
    public event Action OnJump;

    // Uusi hyppyinpuut
    public void QueueJump()
    {
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
    }

    public void ReleaseJump()
    {
        jumpHeld = false;
    }
}
