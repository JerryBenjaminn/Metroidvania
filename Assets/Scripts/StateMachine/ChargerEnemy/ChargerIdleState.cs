using UnityEngine;

public class ChargerIdleState : IState
{
    private ChargerEnemy charger;

    public ChargerIdleState(ChargerEnemy charger)
    {
        this.charger = charger;
    }

    public void Enter()
    {
        Debug.Log("Charger: Entering Idle State");       
    }

    public void Execute()
    {
        if (charger.IsPlayerInRange())
        {
            charger.StateMachine.ChangeState(new ChargerChargeState(charger));
        }
    }

    public void Exit()
    {
        Debug.Log("Charger: Exiting Idle State");
    }
}
