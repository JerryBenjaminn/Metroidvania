using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MetroidvaniaController2D : MonoBehaviour
{
    [Header("Movement")]       
    public float moveSpeed = 8f;
    public float acceleration = 80f;
    public float deceleration = 90f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    bool isJumping = false;

    [Header("Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashTime = 0.15f;
    public float dashCooldown = 0.30f;
    
    [Header("Animation")]
    public Animator anim;
    public float moveAnimThreshold = 0.05f;
    
    Rigidbody2D rb;
    float xInput;
    bool jumpPressed;
    bool jumpHeld;

    float coyoteCounter;
    float jumpBufferCounter;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    float storedGravityScale;

    // Uusi: sallitaanko dash ilmassa
    bool airDashAvailable;

    bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");

        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

        if (IsGrounded)
        {
            coyoteCounter = coyoteTime;
            airDashAvailable = true; // resetoi kun osutaan maahan
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (!isDashing && jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0;
        }

        dashCooldownTimer -= Time.deltaTime;
        if (dashPressed && !isDashing && dashCooldownTimer <= 0f)
        {
            // Dashin saa jos on maassa, tai jos ilmassa mutta airDashAvailable = true
            if (IsGrounded || airDashAvailable)
            {
                StartDash();
                if (!IsGrounded) airDashAvailable = false;
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) EndDash();
        }
        else
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime);
            }
            else if (rb.linearVelocity.y > 0 && !jumpHeld)
            {
                rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime);
            }
        }

        if (Mathf.Abs(xInput) > 0.01f)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Sign(xInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
        if (anim != null)
        {
            bool grounded = IsGrounded;
            bool isMovingHoriz = Mathf.Abs(rb.linearVelocity.x) > moveAnimThreshold;
            bool jump = isJumping;
            
            anim.SetBool("IsGrounded", IsGrounded);
            anim.SetBool("IsMovingHoriz", isMovingHoriz);
            anim.SetBool("IsJumping", isJumping);
            
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = xInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accel = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = Mathf.Clamp(speedDiff * accel, -Mathf.Abs(accel), Mathf.Abs(accel)) * Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteCounter = 0;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        float dir = Mathf.Abs(xInput) > 0.01f
            ? Mathf.Sign(xInput)
            : (transform.localScale.x >= 0f ? 1f : -1f);

        storedGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = storedGravityScale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
