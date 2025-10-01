using UnityEngine;

[RequireComponent(typeof(CharacterMotor))]
public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] float moveAnimThreshold = 0.05f;

    [Header("Run Animation")]
    [Tooltip("Juoksu alkaaa kun |vx| ylittää tämän osuuden maksiminopeudesta")]
    public float runEnterSpeedRatio = 0.70f;
    [Tooltip("Juoksusta palataan käveluun kun |vx| laskee tämän alle")]
    public float runExitSpeedRatio = 0.60f;

    private bool isRunning;

    CharacterMotor motor;

    void Awake() {
        motor = GetComponent<CharacterMotor>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!sr) sr = GetComponentInChildren<SpriteRenderer>();
        motor.OnMovement += HandleMovement;
        motor.OnJump += HandleJump;
    }

    void HandleMovement(Vector2 vel, bool grounded) {
        float speedXAbs = Mathf.Abs(vel.x);
        bool moving = Mathf.Abs(vel.x) > moveAnimThreshold;
        //if (moving) sr.flipX = vel.x < -0.01f;
        sr.flipX = motor.Facing < 0;

        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("IsMoving", moving);
        animator.SetFloat("SpeedXAbs", Mathf.Abs(vel.x));
        animator.SetFloat("SpeedY", vel.y);
        
        // Juoksun hystereesi (rajoita pomppimista kynnyksellä)
        float enter = motor.moveSpeed * runEnterSpeedRatio;
        float exit  = motor.moveSpeed * runExitSpeedRatio;

        if (!isRunning && speedXAbs >= enter) isRunning = true;
        else if (isRunning && speedXAbs <= exit) isRunning = false;

        animator.SetBool("IsRunning", isRunning);
    }

    void HandleJump() {
        animator.SetTrigger("Jump"); // kun hyppy-animaatio on valmis
    }
}

