using UnityEngine;

public class ChargerEnemy : EnemyBase
{
    public StateMachineSystem StateMachine { get; private set; }
    public Transform player;
    public float detectionRange = 5f;
    public float chargeSpeed = 10f;
    public float chargeDuration = 1f;

    private bool isCharging = false;

    protected override void Start()
    {
        base.Start(); // Initialize EnemyBase logic (e.g., health, death handling)
        StateMachine = new StateMachineSystem();
        StateMachine.ChangeState(new ChargerIdleState(this));
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public bool IsPlayerInRange()
    {
        return Vector2.Distance(transform.position, player.position) <= detectionRange;
    }

    public void StartCharge()
    {
        isCharging = true;
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chargeSpeed;
        Invoke(nameof(StopCharge), chargeDuration);
    }

    public bool IsCharging()
    {
        return isCharging;
    }

    private void StopCharge()
    {
        isCharging = false;
        rb.linearVelocity = Vector2.zero;
    }

    protected override void HandleDeath()
    {
        base.HandleDeath(); // Use the death logic from EnemyBase
        // Additional Charger-specific death logic can be added here if needed
    }
}